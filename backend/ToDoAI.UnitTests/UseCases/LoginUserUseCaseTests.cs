using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ToDoAI.Application.Abstractions.DalProviders.RefreshTokenDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.RefreshTokenDalProvider.Models;
using ToDoAI.Application.Abstractions.DalProviders.UserDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.UserDalProvider.Models;
using ToDoAI.Application.Abstractions.Services.AiService;
using ToDoAI.Application.Abstractions.Services.AiService.Models;
using ToDoAI.Application.Services.JwtService;
using ToDoAI.Application.Services.JwtService.Settings;
using ToDoAI.Application.UseCases.CreateUser.Models;
using ToDoAI.Application.UseCases.LoginUser;
using ToDoAI.Application.UseCases.LoginUser.Models;
using ToDoAI.Domain.Enums;

namespace ToDoAI.UnitTests.UseCases;

public sealed class LoginUserUseCaseTests
{
    private readonly Mock<IUserDalProvider> _userDal = new();
    private readonly Mock<IRefreshTokenDalProvider> _refreshTokenDal = new();
    private readonly Mock<IAiMotivationClient> _aiMotivationClient = new();
    private readonly Mock<IJwtService> _jwtService = new();
    private readonly Mock<ILogger<LoginUserUseCase>> _logger = new();

    private LoginUserUseCase CreateUseCase() =>
        new(
            _userDal.Object,
            _refreshTokenDal.Object,
            _aiMotivationClient.Object,
            _jwtService.Object,
            Options.Create(new AuthSettings()),
            _logger.Object);

    private static string HashPassword(Guid userId, string userName, string firstName, string plain)
    {
        var userHash = new UserHash { Id = userId, UserName = userName, FirstName = firstName };
        return new PasswordHasher<UserHash>().HashPassword(userHash, plain);
    }

    private static UserDal MakeUser(bool isEmailConfirmed = true, string? passwordHash = null)
    {
        var userId = Guid.NewGuid();
        return new UserDal
        {
            UserId = userId,
            UserName = "john_doe",
            FirstName = "John",
            Email = "john@example.com",
            IsEmailConfirmed = isEmailConfirmed,
            PasswordHash = passwordHash
                ?? HashPassword(userId, "john_doe", "John", "Password1!")
        };
    }

    // --- User not found ---

    [Fact]
    public async Task LoginUser_WhenUserNotFound_ShouldReturnUserDoesNotExist()
    {
        // Arrange
        _userDal
            .Setup(x => x.GetUser("john_doe", It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserDal?)null);

        var useCase = CreateUseCase();
        var request = new LoginUserBlRequest { UserName = "john_doe", Password = "Password1!" };

        // Act
        var result = await useCase.LoginUser(request, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Should().Be(ErrorCodes.UserDoesNotExist);
        _jwtService.VerifyNoOtherCalls();
        _refreshTokenDal.VerifyNoOtherCalls();
    }

    // --- Email not confirmed ---

    [Fact]
    public async Task LoginUser_WhenEmailNotConfirmed_ShouldReturnEmailNotConfirmed()
    {
        // Arrange
        var user = MakeUser(isEmailConfirmed: false);
        _userDal
            .Setup(x => x.GetUser(user.UserName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var useCase = CreateUseCase();
        var request = new LoginUserBlRequest { UserName = user.UserName, Password = "Password1!" };

        // Act
        var result = await useCase.LoginUser(request, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Should().Be(ErrorCodes.EmailNotConfirmed);
        _jwtService.VerifyNoOtherCalls();
        _refreshTokenDal.VerifyNoOtherCalls();
    }

    // --- Wrong password ---

    [Fact]
    public async Task LoginUser_WhenPasswordIsWrong_ShouldReturnNotAuthorized()
    {
        // Arrange
        var user = MakeUser(isEmailConfirmed: true);
        _userDal
            .Setup(x => x.GetUser(user.UserName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var useCase = CreateUseCase();
        var request = new LoginUserBlRequest { UserName = user.UserName, Password = "WrongPass1!" };

        // Act
        var result = await useCase.LoginUser(request, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Should().Be(ErrorCodes.NotAuthorized);
        _jwtService.VerifyNoOtherCalls();
        _refreshTokenDal.VerifyNoOtherCalls();
    }

    // --- Successful login ---

    [Fact]
    public async Task LoginUser_WhenCredentialsAreValid_ShouldReturnTokensAndStoreRefreshToken()
    {
        // Arrange
        var user = MakeUser(isEmailConfirmed: true);
        _userDal
            .Setup(x => x.GetUser(user.UserName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _jwtService.Setup(x => x.GenerateAccessToken(user)).Returns("access_token");
        _jwtService.Setup(x => x.GenerateRefreshToken(user)).Returns("refresh_token");
        _jwtService.Setup(x => x.HashRefreshToken("refresh_token")).Returns("hashed_refresh_token");
        _aiMotivationClient
            .Setup(x => x.GenerateMotivation(It.IsAny<AiGenerateMotivationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AiGenerateMotivationResult
            {
                UsedAi = true,
                Response = new AiGenerateMotivationResponse
                {
                    Message = "Хороший старт.",
                    Model = "test-model"
                }
            });

        var useCase = CreateUseCase();
        var request = new LoginUserBlRequest { UserName = user.UserName, Password = "Password1!" };

        // Act
        var result = await useCase.LoginUser(request, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Error.Should().BeNull();
        result.AccessToken.Should().Be("access_token");
        result.RefreshToken.Should().Be("refresh_token");
        result.MotivationMessage.Should().Be("Хороший старт.");

        _refreshTokenDal.Verify(
            x => x.CreateRefreshToken(
                It.Is<RefreshTokenRequestDal>(r =>
                    r.UserId == user.UserId &&
                    r.RefreshTokenHash == "hashed_refresh_token" &&
                    r.ExpiresAt > DateTimeOffset.UtcNow),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task LoginUser_WhenRefreshTokenLifetimeConfigured_ShouldUseConfiguredExpiry()
    {
        // Arrange
        var user = MakeUser(isEmailConfirmed: true);
        _userDal
            .Setup(x => x.GetUser(user.UserName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _jwtService.Setup(x => x.GenerateAccessToken(It.IsAny<UserDal>())).Returns("access_token");
        _jwtService.Setup(x => x.GenerateRefreshToken(It.IsAny<UserDal>())).Returns("refresh_token");
        _jwtService.Setup(x => x.HashRefreshToken(It.IsAny<string>())).Returns("hashed");
        _aiMotivationClient
            .Setup(x => x.GenerateMotivation(It.IsAny<AiGenerateMotivationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AiGenerateMotivationResult
            {
                UsedAi = false,
                FallbackReason = "disabled"
            });

        var settings = Options.Create(new AuthSettings { RefreshTokenLifetime = "1.00:00:00" }); // 1 day
        var useCase = new LoginUserUseCase(
            _userDal.Object, _refreshTokenDal.Object, _aiMotivationClient.Object, _jwtService.Object, settings, _logger.Object);

        var request = new LoginUserBlRequest { UserName = user.UserName, Password = "Password1!" };
        var before = DateTimeOffset.UtcNow.AddHours(23);
        var after = DateTimeOffset.UtcNow.AddHours(25);

        // Act
        await useCase.LoginUser(request, CancellationToken.None);

        // Assert — expiry should be ~1 day, not the default 7 days
        _refreshTokenDal.Verify(
            x => x.CreateRefreshToken(
                It.Is<RefreshTokenRequestDal>(r => r.ExpiresAt >= before && r.ExpiresAt <= after),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
