using ToDoAI.Application.Abstractions.DalProviders.GetTaskDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.GetTaskDalProvider.Models;
using ToDoAI.Application.Abstractions.DalProviders.ScheduleDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.ScheduleDalProvider.Models;
using ToDoAI.Application.Abstractions.DalProviders.TaskWorkSessionDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.TaskWorkSessionDalProvider.Models;
using ToDoAI.Application.Abstractions.DalProviders.UpdateTaskStatusDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.UpdateTaskStatusDalProvider.Models;
using ToDoAI.Application.Abstractions.DalProviders.UserDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.UserDalProvider.Models;
using ToDoAI.Application.UseCases.TaskWorkSession;
using ToDoAI.Application.UseCases.TaskWorkSession.Models;
using ToDoAI.Domain.Enums;

namespace ToDoAI.UnitTests.UseCases;

public sealed class CreateTaskWorkSessionUseCaseTests
{
    private readonly Mock<IUserDalProvider> _userDal = new();
    private readonly Mock<IGetTaskDalProvider> _getTaskDal = new();
    private readonly Mock<IScheduleDalProvider> _scheduleDal = new();
    private readonly Mock<ITaskWorkSessionDalProvider> _sessionDal = new();
    private readonly Mock<IUpdateTaskStatusDalProvider> _updateStatusDal = new();

    private CreateTaskWorkSessionUseCase CreateUseCase() =>
        new(_userDal.Object, _getTaskDal.Object, _scheduleDal.Object, _sessionDal.Object, _updateStatusDal.Object);

    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid TaskId = Guid.NewGuid();

    private static LoginUserDal MakeUser() => new()
    {
        UserId = UserId,
        UserName = "john_doe",
        FirstName = "John",
        PasswordHash = "hash"
    };

    private static TaskDal MakeTask(WorkStatus status = WorkStatus.New) => new()
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

    private static TaskWorkSessionDal MakeSession() => new()
    {
        SessionId = Guid.NewGuid(),
        UserId = UserId,
        TaskId = TaskId,
        StartedAt = DateTimeOffset.UtcNow,
        Status = TaskWorkSessionStatus.Open,
        Source = TaskWorkSessionSource.Timer,
        CreatedAt = DateTimeOffset.UtcNow
    };

    private static CreateWorkSessionBlRequest ValidRequest(Guid? scheduleId = null) => new()
    {
        UserId = UserId,
        TaskId = TaskId,
        ScheduleId = scheduleId
    };

    // --- Guard checks ---

    [Fact]
    public async Task CreateTaskWorkSession_WhenUserNotFound_ShouldReturnNotAuthorized()
    {
        // Arrange
        _userDal
            .Setup(x => x.GetUser(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((LoginUserDal?)null);

        var useCase = CreateUseCase();

        // Act
        var result = await useCase.CreateTaskWorkSession(ValidRequest(), CancellationToken.None);

        // Assert
        result.ErrorCode.Should().Be(ErrorCodes.NotAuthorized);
        _getTaskDal.VerifyNoOtherCalls();
        _sessionDal.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CreateTaskWorkSession_WhenTaskNotFound_ShouldReturnTaskNotFound()
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
        var result = await useCase.CreateTaskWorkSession(ValidRequest(), CancellationToken.None);

        // Assert
        result.ErrorCode.Should().Be(ErrorCodes.TaskNotFound);
        _sessionDal.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(WorkStatus.Completed)]
    [InlineData(WorkStatus.Deleted)]
    public async Task CreateTaskWorkSession_WhenTaskIsTerminal_ShouldReturnInvalidTaskStatusTransition(WorkStatus status)
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
        var result = await useCase.CreateTaskWorkSession(ValidRequest(), CancellationToken.None);

        // Assert
        result.ErrorCode.Should().Be(ErrorCodes.InvalidTaskStatusTransition);
        _sessionDal.VerifyNoOtherCalls();
    }

    // --- Schedule validation ---

    [Fact]
    public async Task CreateTaskWorkSession_WhenScheduleNotFound_ShouldReturnScheduleNotFound()
    {
        // Arrange
        var scheduleId = Guid.NewGuid();

        _userDal
            .Setup(x => x.GetUser(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeUser());
        _getTaskDal
            .Setup(x => x.GetTask(TaskId, UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeTask());
        _scheduleDal
            .Setup(x => x.GetSchedule(scheduleId, UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ScheduleDal?)null);

        var useCase = CreateUseCase();

        // Act
        var result = await useCase.CreateTaskWorkSession(ValidRequest(scheduleId), CancellationToken.None);

        // Assert
        result.ErrorCode.Should().Be(ErrorCodes.ScheduleNotFound);
        _sessionDal.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CreateTaskWorkSession_WhenScheduleDoesNotMatchTask_ShouldReturnScheduleDoesNotMatchTask()
    {
        // Arrange
        var scheduleId = Guid.NewGuid();

        _userDal
            .Setup(x => x.GetUser(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeUser());
        _getTaskDal
            .Setup(x => x.GetTask(TaskId, UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeTask());
        _scheduleDal
            .Setup(x => x.GetSchedule(scheduleId, UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ScheduleDal { Id = scheduleId, TaskId = Guid.NewGuid() });

        var useCase = CreateUseCase();

        // Act
        var result = await useCase.CreateTaskWorkSession(ValidRequest(scheduleId), CancellationToken.None);

        // Assert
        result.ErrorCode.Should().Be(ErrorCodes.ScheduleDoesNotMatchTask);
        _sessionDal.VerifyNoOtherCalls();
    }

    // --- Session already exists ---

    [Fact]
    public async Task CreateTaskWorkSession_WhenSessionAlreadyExists_ShouldReturnTaskWorkSessionAlreadyExists()
    {
        // Arrange
        _userDal
            .Setup(x => x.GetUser(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeUser());
        _getTaskDal
            .Setup(x => x.GetTask(TaskId, UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeTask());
        _sessionDal
            .Setup(x => x.CreateTaskWorkSession(It.IsAny<CreateWorkSessionDalRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskWorkSessionDal?)null);

        var useCase = CreateUseCase();

        // Act
        var result = await useCase.CreateTaskWorkSession(ValidRequest(), CancellationToken.None);

        // Assert
        result.ErrorCode.Should().Be(ErrorCodes.TaskWorkSessionAlreadyExists);
    }

    // --- Status auto-transition ---

    [Theory]
    [InlineData(WorkStatus.New)]
    [InlineData(WorkStatus.Todo)]
    public async Task CreateTaskWorkSession_WhenTaskIsNotRunning_ShouldTransitionTaskToRunning(WorkStatus initialStatus)
    {
        // Arrange
        _userDal
            .Setup(x => x.GetUser(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeUser());
        _getTaskDal
            .Setup(x => x.GetTask(TaskId, UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeTask(initialStatus));
        _sessionDal
            .Setup(x => x.CreateTaskWorkSession(It.IsAny<CreateWorkSessionDalRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeSession());
        _updateStatusDal
            .Setup(x => x.UpdateTaskStatus(It.IsAny<UpdateTaskStatusDalRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UpdateTaskStatusDal { WorkStatus = TaskWorkStatusDal.Running });

        var useCase = CreateUseCase();

        // Act
        var result = await useCase.CreateTaskWorkSession(ValidRequest(), CancellationToken.None);

        // Assert
        result.ErrorCode.Should().BeNull();
        _updateStatusDal.Verify(
            x => x.UpdateTaskStatus(
                It.Is<UpdateTaskStatusDalRequest>(r =>
                    r.UserId == UserId &&
                    r.TaskId == TaskId &&
                    r.WorkStatus == TaskWorkStatusDal.Running),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateTaskWorkSession_WhenTaskIsAlreadyRunning_ShouldNotCallUpdateStatus()
    {
        // Arrange
        _userDal
            .Setup(x => x.GetUser(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeUser());
        _getTaskDal
            .Setup(x => x.GetTask(TaskId, UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeTask(WorkStatus.Running));
        _sessionDal
            .Setup(x => x.CreateTaskWorkSession(It.IsAny<CreateWorkSessionDalRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeSession());

        var useCase = CreateUseCase();

        // Act
        var result = await useCase.CreateTaskWorkSession(ValidRequest(), CancellationToken.None);

        // Assert
        result.ErrorCode.Should().BeNull();
        _updateStatusDal.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CreateTaskWorkSession_WhenSuccessful_ShouldReturnSessionDetails()
    {
        // Arrange
        var session = MakeSession();

        _userDal
            .Setup(x => x.GetUser(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeUser());
        _getTaskDal
            .Setup(x => x.GetTask(TaskId, UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeTask(WorkStatus.Running));
        _sessionDal
            .Setup(x => x.CreateTaskWorkSession(It.IsAny<CreateWorkSessionDalRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        var useCase = CreateUseCase();

        // Act
        var result = await useCase.CreateTaskWorkSession(ValidRequest(), CancellationToken.None);

        // Assert
        result.ErrorCode.Should().BeNull();
        result.SessionBlResult.Should().NotBeNull();
        result.SessionBlResult!.SessionId.Should().Be(session.SessionId);
        result.SessionBlResult.TaskId.Should().Be(TaskId);
    }
}
