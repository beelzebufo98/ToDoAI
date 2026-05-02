using ToDoAI.Application.Abstractions.DalProviders.UserDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.UserDalProvider.Models;
using ToDoAI.Application.Abstractions.DalProviders.UserStateDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.UserStateDalProvider.Models;
using ToDoAI.Application.UseCases.UserStateUseCase;
using ToDoAI.Application.UseCases.UserStateUseCase.Models;
using ToDoAI.Domain.Enums;

namespace ToDoAI.UnitTests.UseCases;

public sealed class UserStateUseCaseTests
{
    private readonly Mock<IUserDalProvider> _userDal = new();
    private readonly Mock<IUserStateDalProvider> _userStateDal = new();

    private UserStateUseCase CreateUseCase() =>
        new(_userDal.Object, _userStateDal.Object);

    private static readonly Guid UserId = Guid.NewGuid();

    private static UserDal MakeUser() => new()
    {
        UserId = UserId,
        UserName = "john_doe",
        FirstName = "John",
        PasswordHash = "hash"
    };

    private static CreateUserStateBlRequest ValidRequest() => new()
    {
        UserId = UserId,
        SleepMinutes = 480,
        EnergyLevel = 7,
        StressLevel = 3,
        MotivationLevel = 8,
        ConcentrationLevel = 5
    };

    private static UserStateDal MakeStateDal(DateTimeOffset createdAt) => new()
    {
        UserStateId = Guid.NewGuid(),
        CreatedAt = createdAt,
        SleepMinutes = 420,
        EnergyLevel = 6,
        StressLevel = 4,
        MotivationLevel = 7,
        ConcentrationLevel = 5
    };

    // ── CreateUserState ──────────────────────────────────────────────────────

    [Fact]
    public async Task CreateUserState_WhenUserNotFound_ShouldReturnNotAuthorized()
    {
        // Arrange
        _userDal
            .Setup(x => x.GetUser(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserDal?)null);

        var useCase = CreateUseCase();

        // Act
        var result = await useCase.CreateUserState(ValidRequest(), CancellationToken.None);

        // Assert
        result.ErrorCode.Should().Be(ErrorCodes.NotAuthorized);
        _userStateDal.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CreateUserState_WhenNoExistingState_ShouldCreateNew()
    {
        // Arrange
        _userDal
            .Setup(x => x.GetUser(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeUser());
        _userStateDal
            .Setup(x => x.GetLatestUserState(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserStateDal?)null);
        _userStateDal
            .Setup(x => x.CreateUserState(It.IsAny<UserStateDalRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeStateDal(DateTimeOffset.UtcNow));

        var useCase = CreateUseCase();

        // Act
        await useCase.CreateUserState(ValidRequest(), CancellationToken.None);

        // Assert
        _userStateDal.Verify(
            x => x.CreateUserState(
                It.Is<UserStateDalRequest>(r => r.UserId == UserId),
                It.IsAny<CancellationToken>()),
            Times.Once);
        _userStateDal.Verify(
            x => x.UpdateUserState(It.IsAny<UpdateUserStateDalRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateUserState_WhenExistingStateIsFromDifferentLocalDate_ShouldCreateNew()
    {
        // Arrange
        // 25 hours ago — guaranteed to be a different local date (UTC+3)
        var yesterdayState = MakeStateDal(DateTimeOffset.UtcNow.AddHours(-25));

        _userDal
            .Setup(x => x.GetUser(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeUser());
        _userStateDal
            .Setup(x => x.GetLatestUserState(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(yesterdayState);
        _userStateDal
            .Setup(x => x.CreateUserState(It.IsAny<UserStateDalRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeStateDal(DateTimeOffset.UtcNow));

        var useCase = CreateUseCase();

        // Act
        await useCase.CreateUserState(ValidRequest(), CancellationToken.None);

        // Assert
        _userStateDal.Verify(
            x => x.CreateUserState(It.IsAny<UserStateDalRequest>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _userStateDal.Verify(
            x => x.UpdateUserState(It.IsAny<UpdateUserStateDalRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateUserState_WhenExistingStateIsOlderThan3Hours_ShouldCreateNew()
    {
        // Arrange
        // Same local date but created 4 hours ago → over the 3-hour window
        var oldTodayState = MakeStateDal(DateTimeOffset.UtcNow.AddHours(-4));

        _userDal
            .Setup(x => x.GetUser(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeUser());
        _userStateDal
            .Setup(x => x.GetLatestUserState(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(oldTodayState);
        _userStateDal
            .Setup(x => x.CreateUserState(It.IsAny<UserStateDalRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeStateDal(DateTimeOffset.UtcNow));

        var useCase = CreateUseCase();

        // Act
        await useCase.CreateUserState(ValidRequest(), CancellationToken.None);

        // Assert
        _userStateDal.Verify(
            x => x.CreateUserState(It.IsAny<UserStateDalRequest>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateUserState_WhenExistingStateIsWithin3HourWindow_ShouldUpdate()
    {
        // Arrange
        // Created 1 hour ago on the same date → within the 3-hour update window
        var recentState = MakeStateDal(DateTimeOffset.UtcNow.AddHours(-1));

        _userDal
            .Setup(x => x.GetUser(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeUser());
        _userStateDal
            .Setup(x => x.GetLatestUserState(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(recentState);
        _userStateDal
            .Setup(x => x.UpdateUserState(It.IsAny<UpdateUserStateDalRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeStateDal(DateTimeOffset.UtcNow));

        var useCase = CreateUseCase();

        // Act
        await useCase.CreateUserState(ValidRequest(), CancellationToken.None);

        // Assert
        _userStateDal.Verify(
            x => x.UpdateUserState(
                It.Is<UpdateUserStateDalRequest>(r =>
                    r.StateId == recentState.UserStateId &&
                    r.UserId == UserId),
                It.IsAny<CancellationToken>()),
            Times.Once);
        _userStateDal.Verify(
            x => x.CreateUserState(It.IsAny<UserStateDalRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // ── GetLatestUserState ───────────────────────────────────────────────────

    [Fact]
    public async Task GetLatestUserState_WhenUserNotFound_ShouldReturnNotAuthorized()
    {
        // Arrange
        _userDal
            .Setup(x => x.GetUser(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserDal?)null);

        var useCase = CreateUseCase();

        // Act
        var result = await useCase.GetLatestUserState(UserId, CancellationToken.None);

        // Assert
        result.ErrorCode.Should().Be(ErrorCodes.NotAuthorized);
    }

    [Fact]
    public async Task GetLatestUserState_WhenNoStateExists_ShouldReturnUserStateNotFound()
    {
        // Arrange
        _userDal
            .Setup(x => x.GetUser(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeUser());
        _userStateDal
            .Setup(x => x.GetLatestUserState(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserStateDal?)null);

        var useCase = CreateUseCase();

        // Act
        var result = await useCase.GetLatestUserState(UserId, CancellationToken.None);

        // Assert
        result.ErrorCode.Should().Be(ErrorCodes.UserStateNotFound);
        result.UserState.Should().BeNull();
    }

    [Fact]
    public async Task GetLatestUserState_WhenStateExists_ShouldReturnMappedState()
    {
        // Arrange
        var state = MakeStateDal(DateTimeOffset.UtcNow);
        _userDal
            .Setup(x => x.GetUser(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeUser());
        _userStateDal
            .Setup(x => x.GetLatestUserState(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(state);

        var useCase = CreateUseCase();

        // Act
        var result = await useCase.GetLatestUserState(UserId, CancellationToken.None);

        // Assert
        result.ErrorCode.Should().BeNull();
        result.UserState.Should().NotBeNull();
        result.UserState!.SleepMinutes.Should().Be(state.SleepMinutes);
        result.UserState.EnergyLevel.Should().Be(state.EnergyLevel);
    }
}
