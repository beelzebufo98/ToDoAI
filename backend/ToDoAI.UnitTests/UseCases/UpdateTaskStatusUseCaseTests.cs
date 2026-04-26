using ToDoAI.Application.Abstractions.DalProviders.UpdateTaskStatusDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.UpdateTaskStatusDalProvider.Models;
using ToDoAI.Application.Abstractions.DalProviders.UserDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.UserDalProvider.Models;
using ToDoAI.Application.UseCases.GetTask.Models;
using ToDoAI.Application.UseCases.UpdateTaskStatus;
using ToDoAI.Application.UseCases.UpdateTaskStatus.Models;
using ToDoAI.Domain.Enums;

namespace ToDoAI.UnitTests.UseCases;

public sealed class UpdateTaskStatusUseCaseTests
{
    private readonly Mock<IUpdateTaskStatusDalProvider> _updateStatusDal = new();
    private readonly Mock<IUserDalProvider> _userDal = new();

    private UpdateTaskStatusUseCase CreateUseCase() =>
        new(_updateStatusDal.Object, _userDal.Object);

    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid TaskId = Guid.NewGuid();

    private static LoginUserDal MakeUser() => new()
    {
        UserId = UserId,
        UserName = "john_doe",
        FirstName = "John",
        PasswordHash = "hash"
    };

    private static UpdateTaskStatusBlRequest MakeRequest(TaskWorkStatusBlFilters status) => new()
    {
        UserId = UserId,
        TaskId = TaskId,
        TaskWorkStatus = status
    };

    // --- User not found ---

    [Fact]
    public async Task UpdateTaskStatus_WhenUserNotFound_ShouldReturnNotAuthorized()
    {
        // Arrange
        _userDal
            .Setup(x => x.GetUser(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((LoginUserDal?)null);

        var useCase = CreateUseCase();

        // Act
        var result = await useCase.UpdateTaskStatus(MakeRequest(TaskWorkStatusBlFilters.Todo), CancellationToken.None);

        // Assert
        result.ErrorCode.Should().Be(ErrorCodes.NotAuthorized);
        _updateStatusDal.VerifyNoOtherCalls();
    }

    // --- Task not found ---

    [Fact]
    public async Task UpdateTaskStatus_WhenTaskNotFound_ShouldReturnTaskNotFound()
    {
        // Arrange
        _userDal
            .Setup(x => x.GetUser(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeUser());
        _updateStatusDal
            .Setup(x => x.UpdateTaskStatus(It.IsAny<UpdateTaskStatusDalRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UpdateTaskStatusDal?)null);

        var useCase = CreateUseCase();

        // Act
        var result = await useCase.UpdateTaskStatus(MakeRequest(TaskWorkStatusBlFilters.Todo), CancellationToken.None);

        // Assert
        result.ErrorCode.Should().Be(ErrorCodes.TaskNotFound);
    }

    // --- DAL propagates error ---

    [Fact]
    public async Task UpdateTaskStatus_WhenDalReturnsError_ShouldPropagateErrorCode()
    {
        // Arrange
        _userDal
            .Setup(x => x.GetUser(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeUser());
        _updateStatusDal
            .Setup(x => x.UpdateTaskStatus(It.IsAny<UpdateTaskStatusDalRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UpdateTaskStatusDal { ErrorCode = ErrorCodes.InvalidTaskStatusTransition });

        var useCase = CreateUseCase();

        // Act
        var result = await useCase.UpdateTaskStatus(MakeRequest(TaskWorkStatusBlFilters.New), CancellationToken.None);

        // Assert
        result.ErrorCode.Should().Be(ErrorCodes.InvalidTaskStatusTransition);
    }

    // --- Successful update ---

    [Theory]
    [InlineData(TaskWorkStatusBlFilters.New,       TaskWorkStatusDal.New,       TaskWorkStatusBlFilters.New)]
    [InlineData(TaskWorkStatusBlFilters.Todo,      TaskWorkStatusDal.Todo,      TaskWorkStatusBlFilters.Todo)]
    [InlineData(TaskWorkStatusBlFilters.Running,   TaskWorkStatusDal.Running,   TaskWorkStatusBlFilters.Running)]
    [InlineData(TaskWorkStatusBlFilters.Completed, TaskWorkStatusDal.Completed, TaskWorkStatusBlFilters.Completed)]
    public async Task UpdateTaskStatus_WhenSuccessful_ShouldMapStatusBidirectionally(
        TaskWorkStatusBlFilters requestedStatus,
        TaskWorkStatusDal expectedDalStatus,
        TaskWorkStatusBlFilters expectedResultStatus)
    {
        // Arrange
        _userDal
            .Setup(x => x.GetUser(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeUser());
        _updateStatusDal
            .Setup(x => x.UpdateTaskStatus(It.IsAny<UpdateTaskStatusDalRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UpdateTaskStatusDal { WorkStatus = expectedDalStatus });

        var useCase = CreateUseCase();

        // Act
        var result = await useCase.UpdateTaskStatus(MakeRequest(requestedStatus), CancellationToken.None);

        // Assert
        result.ErrorCode.Should().BeNull();
        result.WorkStatus.Should().Be(expectedResultStatus);

        _updateStatusDal.Verify(
            x => x.UpdateTaskStatus(
                It.Is<UpdateTaskStatusDalRequest>(r =>
                    r.UserId == UserId &&
                    r.TaskId == TaskId &&
                    r.WorkStatus == expectedDalStatus),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
