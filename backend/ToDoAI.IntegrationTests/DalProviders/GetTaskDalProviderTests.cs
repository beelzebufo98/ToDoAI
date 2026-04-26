using ToDoAI.Application.UseCases.GetTask.Models;
using ToDoAI.Domain.Entities;
using ToDoAI.Domain.Enums;
using ToDoAI.Infrastructure.DalProviders.GetTaskDalProvider;
using ToDoAI.Infrastructure.Data;
using ToDoAI.IntegrationTests.Fixtures;

namespace ToDoAI.IntegrationTests.DalProviders;

[Collection("Database")]
public sealed class GetTaskDalProviderTests : IAsyncLifetime
{
    private readonly IDbContextFactory<ToDoAIDbContext> _dbContextFactory;
    private readonly GetTaskDalProvider _dal;
    private readonly Guid _userId = Guid.NewGuid();

    public GetTaskDalProviderTests(PostgresFixture fixture)
    {
        _dbContextFactory = fixture.DbContextFactory;
        _dal = new GetTaskDalProvider(_dbContextFactory);
    }

    public async Task InitializeAsync()
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        db.Users.Add(new UserEntity
        {
            Id = _userId,
            UserName = $"user_{_userId:N}",
            FirstName = "Test",
            PasswordHash = "hash"
        });
        await db.SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        await db.Tasks.Where(t => t.UserId == _userId).ExecuteDeleteAsync();
        await db.Users.Where(u => u.Id == _userId).ExecuteDeleteAsync();
    }

    private TaskEntity MakeTask(
        WorkStatus status = WorkStatus.New,
        int complexityLevel = 5,
        int priority = 3,
        Guid? userId = null)
    {
        return new TaskEntity
        {
            Id = Guid.NewGuid(),
            Title = "Test task",
            Description = "Desc",
            EstimatedMinutes = 60,
            ComplexityLevel = complexityLevel,
            Priority = priority,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            DeadlineAt = DateTimeOffset.UtcNow.AddDays(7),
            WorkStatus = status,
            UserId = userId ?? _userId
        };
    }

    private static TaskFiltersBlRequest DefaultFilters()
    {
        return new TaskFiltersBlRequest { PageSize = 10 };
    }

    [Fact(DisplayName = "Задача существует — возвращает задачу с корректными полями")]
    public async Task GetTask_WhenTaskExists_ShouldReturnMappedTask()
    {
        // Arrange
        var task = MakeTask();
        await using (var db = await _dbContextFactory.CreateDbContextAsync())
        {
            db.Tasks.Add(task);
            await db.SaveChangesAsync();
        }

        // Act
        var result = await _dal.GetTask(task.Id, _userId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(task.Id);
        result.Title.Should().Be(task.Title);
        result.ComplexityLevel.Should().Be(task.ComplexityLevel);
        result.WorkStatus.Should().Be(WorkStatus.New);
    }

    [Fact(DisplayName = "Несуществующий ID задачи — возвращает null")]
    public async Task GetTask_WhenTaskIdDoesNotExist_ShouldReturnNull()
    {
        // Act
        var result = await _dal.GetTask(Guid.NewGuid(), _userId, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact(DisplayName = "Задача принадлежит другому пользователю — возвращает null")]
    public async Task GetTask_WhenTaskBelongsToDifferentUser_ShouldReturnNull()
    {
        // Arrange
        var otherId = Guid.NewGuid();
        var task = MakeTask(userId: otherId);

        await using (var db = await _dbContextFactory.CreateDbContextAsync())
        {
            db.Users.Add(new UserEntity
                { Id = otherId, UserName = $"user_{otherId:N}", FirstName = "Other", PasswordHash = "hash" });
            db.Tasks.Add(task);
            await db.SaveChangesAsync();
        }

        // Act
        var result = await _dal.GetTask(task.Id, _userId, CancellationToken.None);

        // Assert
        result.Should().BeNull();

        await using (var db = await _dbContextFactory.CreateDbContextAsync())
        {
            await db.Tasks.Where(t => t.Id == task.Id).ExecuteDeleteAsync();
            await db.Users.Where(u => u.Id == otherId).ExecuteDeleteAsync();
        }
    }

    [Fact(DisplayName = "Задача со статусом Deleted — возвращает null")]
    public async Task GetTask_WhenTaskIsDeleted_ShouldReturnNull()
    {
        // Arrange
        var task = MakeTask(WorkStatus.Deleted);
        await using (var db = await _dbContextFactory.CreateDbContextAsync())
        {
            db.Tasks.Add(task);
            await db.SaveChangesAsync();
        }

        // Act
        var result = await _dal.GetTask(task.Id, _userId, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact(DisplayName = "Без фильтра статуса — включает New/Todo/Running, исключает Completed/Deleted")]
    public async Task GetTasks_WhenNoStatusFilter_ShouldIncludeNewTodoRunningAndExcludeCompletedDeleted()
    {
        // Arrange
        var newTask = MakeTask(WorkStatus.New);
        var todoTask = MakeTask(WorkStatus.Todo);
        var runningTask = MakeTask(WorkStatus.Running);
        var completedTask = MakeTask(WorkStatus.Completed);
        var deletedTask = MakeTask(WorkStatus.Deleted);

        await using (var db = await _dbContextFactory.CreateDbContextAsync())
        {
            db.Tasks.AddRange(newTask, todoTask, runningTask, completedTask, deletedTask);
            await db.SaveChangesAsync();
        }

        // Act
        var result = await _dal.GetTasks(_userId, DefaultFilters(), CancellationToken.None);

        // Assert
        result.Should().HaveCount(3);
        result.Select(t => t.Id).Should().Contain([newTask.Id, todoTask.Id, runningTask.Id]);
        result.Select(t => t.Id).Should().NotContain([completedTask.Id, deletedTask.Id]);
    }

    [Theory(DisplayName = "Фильтр по статусу — возвращает только задачи с указанным статусом")]
    [InlineData(TaskWorkStatusBlFilters.New, WorkStatus.New)]
    [InlineData(TaskWorkStatusBlFilters.Todo, WorkStatus.Todo)]
    [InlineData(TaskWorkStatusBlFilters.Running, WorkStatus.Running)]
    [InlineData(TaskWorkStatusBlFilters.Completed, WorkStatus.Completed)]
    public async Task GetTasks_WhenStatusFilterApplied_ShouldReturnOnlyMatchingTasks(
        TaskWorkStatusBlFilters filter, WorkStatus expectedStatus)
    {
        // Arrange
        var matching = MakeTask(expectedStatus);
        var notMatching = MakeTask(expectedStatus == WorkStatus.New ? WorkStatus.Todo : WorkStatus.New);

        await using (var db = await _dbContextFactory.CreateDbContextAsync())
        {
            db.Tasks.AddRange(matching, notMatching);
            await db.SaveChangesAsync();
        }

        var filters = new TaskFiltersBlRequest { PageSize = 10, WorkStatus = filter };

        // Act
        var result = await _dal.GetTasks(_userId, filters, CancellationToken.None);

        // Assert
        result.Should().Contain(t => t.Id == matching.Id);
        result.Should().NotContain(t => t.Id == notMatching.Id);
    }

    [Fact(DisplayName = "Результаты изолированы по userId — задачи других пользователей не попадают")]
    public async Task GetTasks_ShouldIsolateResultsByUserId()
    {
        // Arrange
        var myTask = MakeTask();
        var otherId = Guid.NewGuid();
        var otherTask = MakeTask(userId: otherId);

        await using (var db = await _dbContextFactory.CreateDbContextAsync())
        {
            db.Users.Add(new UserEntity
                { Id = otherId, UserName = $"user_{otherId:N}", FirstName = "Other", PasswordHash = "hash" });
            db.Tasks.AddRange(myTask, otherTask);
            await db.SaveChangesAsync();
        }

        // Act
        var result = await _dal.GetTasks(_userId, DefaultFilters(), CancellationToken.None);

        // Assert
        result.Should().Contain(t => t.Id == myTask.Id);
        result.Should().NotContain(t => t.Id == otherTask.Id);

        await using (var db = await _dbContextFactory.CreateDbContextAsync())
        {
            await db.Tasks.Where(t => t.Id == otherTask.Id).ExecuteDeleteAsync();
            await db.Users.Where(u => u.Id == otherId).ExecuteDeleteAsync();
        }
    }

    [Fact(DisplayName = "Постраничная выборка — страницы не пересекаются")]
    public async Task GetTasks_WhenPaged_ShouldReturnNonOverlappingPages()
    {
        // Arrange — staggered CreatedAt for deterministic default (CreatedAt asc) ordering
        var tasks = Enumerable.Range(1, 5).Select(i => new TaskEntity
        {
            Id = Guid.NewGuid(),
            Title = $"Task {i}",
            Description = "Desc",
            EstimatedMinutes = 30,
            ComplexityLevel = 5,
            Priority = 3,
            CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-i),
            UpdatedAt = DateTimeOffset.UtcNow,
            DeadlineAt = DateTimeOffset.UtcNow.AddDays(7),
            WorkStatus = WorkStatus.New,
            UserId = _userId
        }).ToList();

        await using (var db = await _dbContextFactory.CreateDbContextAsync())
        {
            db.Tasks.AddRange(tasks);
            await db.SaveChangesAsync();
        }

        // Act
        var pageOne = await _dal.GetTasks(_userId, new TaskFiltersBlRequest { PageSize = 3, Page = 1 },
            CancellationToken.None);
        var pageTwo = await _dal.GetTasks(_userId, new TaskFiltersBlRequest { PageSize = 3, Page = 2 },
            CancellationToken.None);

        // Assert
        pageOne.Should().HaveCount(3);
        pageTwo.Should().HaveCount(2);
        pageOne.Select(t => t.Id).Intersect(pageTwo.Select(t => t.Id)).Should().BeEmpty();
    }

    [Fact(DisplayName = "Сортировка по сложности убыванием — первой идёт задача с наибольшей сложностью")]
    public async Task GetTasks_WhenSortedByComplexityLevelDescending_ShouldReturnHighestComplexityFirst()
    {
        // Arrange
        var low = MakeTask(complexityLevel: 2);
        var mid = MakeTask(complexityLevel: 5);
        var high = MakeTask(complexityLevel: 9);

        await using (var db = await _dbContextFactory.CreateDbContextAsync())
        {
            db.Tasks.AddRange(low, mid, high);
            await db.SaveChangesAsync();
        }

        var filters = new TaskFiltersBlRequest
        {
            PageSize = 10,
            SortBy = TaskSortByBlFilters.ComplexityLevel,
            SortType = TaskTypeSortBl.Desc
        };

        // Act
        var result = await _dal.GetTasks(_userId, filters, CancellationToken.None);

        // Assert
        result.Should().BeInDescendingOrder(t => t.ComplexityLevel);
    }
}