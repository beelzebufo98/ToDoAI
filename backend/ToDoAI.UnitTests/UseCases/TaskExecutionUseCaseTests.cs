using ToDoAI.Application.Abstractions.DalProviders.GetTaskDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.GetTaskDalProvider.Models;
using ToDoAI.Application.Abstractions.DalProviders.ScheduleDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.ScheduleDalProvider.Models;
using ToDoAI.Application.Abstractions.DalProviders.TaskExecutionDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.TaskExecutionDalProvider.Models;
using ToDoAI.Application.Abstractions.DalProviders.UserDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.UserDalProvider.Models;
using ToDoAI.Application.Abstractions.Services.AiService;
using ToDoAI.Application.Abstractions.Services.AiService.Models;
using ToDoAI.Application.UseCases.TaskExecutionUseCase;
using ToDoAI.Application.UseCases.TaskExecutionUseCase.Models;
using ToDoAI.Domain.Enums;

namespace ToDoAI.UnitTests.UseCases;

public sealed class TaskExecutionUseCaseTests
{
    private readonly Mock<ITaskExecutionDalProvider> _executionDal = new();
    private readonly Mock<IUserDalProvider> _userDal = new();
    private readonly Mock<IGetTaskDalProvider> _getTaskDal = new();
    private readonly Mock<IScheduleDalProvider> _scheduleDal = new();
    private readonly Mock<IAiMotivationClient> _aiMotivationClient = new();

    private TaskExecutionUseCase CreateUseCase() =>
        new(_executionDal.Object, _userDal.Object, _getTaskDal.Object, _scheduleDal.Object, _aiMotivationClient.Object);

    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid TaskId = Guid.NewGuid();

    private static UserDal MakeUser() => new()
    {
        UserId = UserId,
        UserName = "john_doe",
        FirstName = "John",
        PasswordHash = "hash"
    };

    private static TaskDal MakeTask(WorkStatus status = WorkStatus.Completed) => new()
    {
        Id = TaskId,
        Title = "Task",
        Description = "Desc",
        EstimatedMinutes = 60,
        WorkStatus = status,
        DeadlineAt = DateTimeOffset.UtcNow.AddDays(1),
        CreatedAt = DateTimeOffset.UtcNow,
        UpdatedAt = DateTimeOffset.UtcNow
    };

    private static TaskExecutionBlRequest ValidRequest(Guid? scheduleId = null) => new()
    {
        UserId = UserId,
        TaskId = TaskId,
        ScheduleId = scheduleId,
        ActualMinutes = 55,
        EnergyAfter = 6,
        StressAfter = 4
    };

    private static TaskExecutionDal MakeExecutionDal(Guid? scheduleId = null) => new()
    {
        TaskExecutionId = Guid.NewGuid(),
        TaskId = TaskId,
        ScheduleId = scheduleId,
        ActualMinutes = 55,
        EnergyAfter = 6,
        StressAfter = 4,
        CreatedAt = DateTimeOffset.UtcNow
    };

    // --- Guard checks ---

    [Fact]
    public async Task CreateTaskExecution_WhenUserNotFound_ShouldReturnNotAuthorized()
    {
        // Arrange
        _userDal
            .Setup(x => x.GetUser(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserDal?)null);

        var useCase = CreateUseCase();

        // Act
        var result = await useCase.CreateTaskExecution(ValidRequest(), CancellationToken.None);

        // Assert
        result.ErrorCode.Should().Be(ErrorCodes.NotAuthorized);
        _getTaskDal.VerifyNoOtherCalls();
        _executionDal.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CreateTaskExecution_WhenTaskNotFound_ShouldReturnTaskNotFound()
    {
        // Arrange
        _userDal
            .Setup(x => x.GetUser(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeUser());
        _getTaskDal
            .Setup(x => x.GetTask(TaskId, UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskDal?)null);

        var useCase = CreateUseCase();

        // Act
        var result = await useCase.CreateTaskExecution(ValidRequest(), CancellationToken.None);

        // Assert
        result.ErrorCode.Should().Be(ErrorCodes.TaskNotFound);
        _executionDal.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(WorkStatus.New)]
    [InlineData(WorkStatus.Todo)]
    [InlineData(WorkStatus.Running)]
    public async Task CreateTaskExecution_WhenTaskNotCompleted_ShouldReturnTaskShouldBeCompleted(WorkStatus status)
    {
        // Arrange
        _userDal
            .Setup(x => x.GetUser(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeUser());
        _getTaskDal
            .Setup(x => x.GetTask(TaskId, UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeTask(status));

        var useCase = CreateUseCase();

        // Act
        var result = await useCase.CreateTaskExecution(ValidRequest(), CancellationToken.None);

        // Assert
        result.ErrorCode.Should().Be(ErrorCodes.TaskShouldBeCompleted);
        _executionDal.VerifyNoOtherCalls();
    }

    // --- Schedule validation ---

    [Fact]
    public async Task CreateTaskExecution_WhenScheduleNotFound_ShouldReturnScheduleNotFound()
    {
        // Arrange
        var scheduleId = Guid.NewGuid();

        _userDal
            .Setup(x => x.GetUser(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeUser());
        _getTaskDal
            .Setup(x => x.GetTask(TaskId, UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeTask(WorkStatus.Completed));
        _scheduleDal
            .Setup(x => x.GetSchedule(scheduleId, UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ScheduleDal?)null);

        var useCase = CreateUseCase();

        // Act
        var result = await useCase.CreateTaskExecution(ValidRequest(scheduleId), CancellationToken.None);

        // Assert
        result.ErrorCode.Should().Be(ErrorCodes.ScheduleNotFound);
        _executionDal.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CreateTaskExecution_WhenScheduleDoesNotMatchTask_ShouldReturnScheduleDoesNotMatchTask()
    {
        // Arrange
        var scheduleId = Guid.NewGuid();
        var differentTaskId = Guid.NewGuid();

        _userDal
            .Setup(x => x.GetUser(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeUser());
        _getTaskDal
            .Setup(x => x.GetTask(TaskId, UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeTask(WorkStatus.Completed));
        _scheduleDal
            .Setup(x => x.GetSchedule(scheduleId, UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ScheduleDal { Id = scheduleId, TaskId = differentTaskId });

        var useCase = CreateUseCase();

        // Act
        var result = await useCase.CreateTaskExecution(ValidRequest(scheduleId), CancellationToken.None);

        // Assert
        result.ErrorCode.Should().Be(ErrorCodes.ScheduleDoesNotMatchTask);
        _executionDal.VerifyNoOtherCalls();
    }

    // --- Duplicate check ---

    [Fact]
    public async Task CreateTaskExecution_WhenExecutionAlreadyExists_ShouldReturnTaskExecutionAlreadyExists()
    {
        // Arrange
        _userDal
            .Setup(x => x.GetUser(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeUser());
        _getTaskDal
            .Setup(x => x.GetTask(TaskId, UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeTask(WorkStatus.Completed));
        _executionDal
            .Setup(x => x.CreateTaskExecution(It.IsAny<TaskExecutionDalRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskExecutionDal?)null);

        var useCase = CreateUseCase();

        // Act
        var result = await useCase.CreateTaskExecution(ValidRequest(), CancellationToken.None);

        // Assert
        result.ErrorCode.Should().Be(ErrorCodes.TaskExecutionAlreadyExists);
    }

    // --- Happy paths ---

    [Fact]
    public async Task CreateTaskExecution_WhenValidWithoutSchedule_ShouldRecordExecutionAndReturnResult()
    {
        // Arrange
        _userDal
            .Setup(x => x.GetUser(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeUser());
        _getTaskDal
            .Setup(x => x.GetTask(TaskId, UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeTask(WorkStatus.Completed));
        _executionDal
            .Setup(x => x.CreateTaskExecution(It.IsAny<TaskExecutionDalRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeExecutionDal());
        _aiMotivationClient
            .Setup(x => x.GenerateMotivation(It.IsAny<AiGenerateMotivationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AiGenerateMotivationResult
            {
                UsedAi = true,
                Response = new AiGenerateMotivationResponse
                {
                    Message = "Хорошая работа.",
                    Model = "test-model"
                }
            });

        var useCase = CreateUseCase();

        // Act
        var result = await useCase.CreateTaskExecution(ValidRequest(), CancellationToken.None);

        // Assert
        result.ErrorCode.Should().BeNull();
        result.TaskExecutionResult.Should().NotBeNull();
        result.TaskExecutionResult!.TaskId.Should().Be(TaskId);
        result.TaskExecutionResult.MotivationMessage.Should().Be("Хорошая работа.");
        _scheduleDal.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CreateTaskExecution_WhenValidWithMatchingSchedule_ShouldPassScheduleIdToExecution()
    {
        // Arrange
        var scheduleId = Guid.NewGuid();

        _userDal
            .Setup(x => x.GetUser(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeUser());
        _getTaskDal
            .Setup(x => x.GetTask(TaskId, UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeTask(WorkStatus.Completed));
        _scheduleDal
            .Setup(x => x.GetSchedule(scheduleId, UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ScheduleDal { Id = scheduleId, TaskId = TaskId });
        _executionDal
            .Setup(x => x.CreateTaskExecution(It.IsAny<TaskExecutionDalRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeExecutionDal(scheduleId));
        _aiMotivationClient
            .Setup(x => x.GenerateMotivation(It.IsAny<AiGenerateMotivationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AiGenerateMotivationResult
            {
                UsedAi = false,
                FallbackReason = "disabled"
            });

        var useCase = CreateUseCase();

        // Act
        var result = await useCase.CreateTaskExecution(ValidRequest(scheduleId), CancellationToken.None);

        // Assert
        result.ErrorCode.Should().BeNull();
        _executionDal.Verify(
            x => x.CreateTaskExecution(
                It.Is<TaskExecutionDalRequest>(r => r.ScheduleId == scheduleId && r.TaskId == TaskId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
