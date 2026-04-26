using ToDoAI.Application.Abstractions.DalProviders.CreateTaskDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.CreateTaskDalProvider.Models;
using ToDoAI.Application.Abstractions.DalProviders.UserDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.UserDalProvider.Models;
using ToDoAI.Application.UseCases.CreateTask;
using ToDoAI.Application.UseCases.CreateTask.Models;
using ToDoAI.Domain.Enums;

namespace ToDoAI.UnitTests.UseCases;

public sealed class CreateTaskUseCaseTests
{
    private readonly Mock<IUserDalProvider> _userDal = new();
    private readonly Mock<ICreateTaskDalProvider> _createTaskDal = new();

    private CreateTaskUseCase CreateUseCase() =>
        new(_userDal.Object, _createTaskDal.Object);

    private static readonly Guid UserId = Guid.NewGuid();

    private static LoginUserDal MakeUser() => new()
    {
        UserId = UserId,
        UserName = "john_doe",
        FirstName = "John",
        PasswordHash = "hash"
    };

    private static CreateTaskBlRequest ValidRequest() => new()
    {
        UserId = UserId,
        Title = "Fix the bug",
        Description = "There is a null ref somewhere",
        EstimatedMinutes = 60,
        ComplexityLevel = 5,
        Priority = 3,
        DeadlineAt = DateTimeOffset.UtcNow.AddDays(7)
    };

    // --- User not found ---

    [Fact]
    public async Task CreateTask_WhenUserNotFound_ShouldReturnNotAuthorized()
    {
        // Arrange
        _userDal
            .Setup(x => x.GetUser(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((LoginUserDal?)null);

        var useCase = CreateUseCase();

        // Act
        var result = await useCase.CreateTask(ValidRequest(), CancellationToken.None);

        // Assert
        result.ErrorCode.Should().Be(ErrorCodes.NotAuthorized);
        result.TaskId.Should().BeNull();
        _createTaskDal.VerifyNoOtherCalls();
    }

    // --- Successful creation ---

    [Fact]
    public async Task CreateTask_WhenUserExists_ShouldCallDalWithCorrectData()
    {
        // Arrange
        var user = MakeUser();
        var createdTaskId = Guid.NewGuid();

        _userDal
            .Setup(x => x.GetUser(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _createTaskDal
            .Setup(x => x.CreateTask(It.IsAny<CreateTaskReqDal>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CreateTaskDal(createdTaskId));

        var request = ValidRequest();
        var useCase = CreateUseCase();

        // Act
        var result = await useCase.CreateTask(request, CancellationToken.None);

        // Assert
        result.ErrorCode.Should().BeNull();
        result.TaskId.Should().Be(createdTaskId);

        _createTaskDal.Verify(
            x => x.CreateTask(
                It.Is<CreateTaskReqDal>(r =>
                    r.UserId == UserId &&
                    r.Title == request.Title &&
                    r.Description == request.Description &&
                    r.EstimatedMinutes == request.EstimatedMinutes &&
                    r.ComplexityLevel == request.ComplexityLevel &&
                    r.PriorityLevel == request.Priority &&
                    r.DeadlineAt == request.DeadlineAt),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateTask_WhenUserExists_ShouldGenerateUniqueTaskIdPerCall()
    {
        // Arrange
        var capturedIds = new List<Guid>();

        _userDal
            .Setup(x => x.GetUser(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeUser());
        _createTaskDal
            .Setup(x => x.CreateTask(It.IsAny<CreateTaskReqDal>(), It.IsAny<CancellationToken>()))
            .Callback<CreateTaskReqDal, CancellationToken>((req, _) => capturedIds.Add(req.TaskId))
            .ReturnsAsync(new CreateTaskDal(Guid.NewGuid()));

        var useCase = CreateUseCase();

        // Act
        await useCase.CreateTask(ValidRequest(), CancellationToken.None);
        await useCase.CreateTask(ValidRequest(), CancellationToken.None);

        // Assert — each call assigns a distinct TaskId to the DAL request
        capturedIds.Should().HaveCount(2);
        capturedIds[0].Should().NotBe(capturedIds[1]);
    }
}
