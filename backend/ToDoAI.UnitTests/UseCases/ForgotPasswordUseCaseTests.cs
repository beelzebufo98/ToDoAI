using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ToDoAI.Application.Abstractions.DalProviders.PasswordResetDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.PasswordResetDalProvider.Models;
using ToDoAI.Application.Abstractions.DalProviders.UserDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.UserDalProvider.Models;
using ToDoAI.Application.Services.EmailService;
using ToDoAI.Application.Services.EmailService.Settings;
using ToDoAI.Application.UseCases.ForgotPassword;

namespace ToDoAI.UnitTests.UseCases;

public sealed class ForgotPasswordUseCaseTests
{
    private readonly Mock<IUserDalProvider> _userDal = new();
    private readonly Mock<IPasswordResetDalProvider> _passwordResetDal = new();
    private readonly Mock<IEmailService> _emailService = new();
    private readonly Mock<ILogger<ForgotPasswordUseCase>> _logger = new();

    private ForgotPasswordUseCase CreateUseCase(bool emailEnabled = true) =>
        new(
            _userDal.Object,
            _passwordResetDal.Object,
            _emailService.Object,
            Options.Create(new EmailSettings { Enabled = emailEnabled }),
            _logger.Object);

    private static LoginUserDal MakeUser(bool isEmailConfirmed = true) => new()
    {
        UserId = Guid.NewGuid(),
        UserName = "john_doe",
        FirstName = "John",
        Email = "john@example.com",
        PasswordHash = "hash",
        IsEmailConfirmed = isEmailConfirmed
    };

    private static PasswordResetDal MakeExistingReset(Guid userId, int sentSecondsAgo) => new()
    {
        Id = Guid.NewGuid(),
        UserId = userId,
        CodeHash = "some_hash",
        ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(10),
        SentAt = DateTimeOffset.UtcNow.AddSeconds(-sentSecondsAgo),
        Attempts = 0
    };

    // --- Silent exits ---

    [Fact]
    public async Task ForgotPassword_WhenUserNotFound_ShouldDoNothing()
    {
        // Arrange
        _userDal
            .Setup(x => x.GetUserByEmail(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((LoginUserDal?)null);

        var useCase = CreateUseCase();

        // Act
        await useCase.ForgotPassword("ghost@example.com", CancellationToken.None);

        // Assert
        _passwordResetDal.VerifyNoOtherCalls();
        _emailService.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ForgotPassword_WhenEmailNotConfirmed_ShouldDoNothing()
    {
        // Arrange
        var user = MakeUser(isEmailConfirmed: false);
        _userDal
            .Setup(x => x.GetUserByEmail(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var useCase = CreateUseCase();

        // Act
        await useCase.ForgotPassword(user.Email!, CancellationToken.None);

        // Assert
        _passwordResetDal.VerifyNoOtherCalls();
        _emailService.VerifyNoOtherCalls();
    }

    // --- Cooldown ---

    [Fact]
    public async Task ForgotPassword_WhenWithinCooldown_ShouldDoNothing()
    {
        // Arrange
        var user = MakeUser();
        var recentReset = MakeExistingReset(user.UserId, sentSecondsAgo: 30);

        _userDal
            .Setup(x => x.GetUserByEmail(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _passwordResetDal
            .Setup(x => x.GetPasswordReset(user.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(recentReset);

        var useCase = CreateUseCase();

        // Act
        await useCase.ForgotPassword(user.Email!, CancellationToken.None);

        // Assert
        _passwordResetDal.Verify(
            x => x.ReplacePasswordReset(It.IsAny<PasswordResetRequestDal>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _emailService.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ForgotPassword_WhenCooldownExpired_ShouldReplaceResetAndSendEmail()
    {
        // Arrange
        var user = MakeUser();
        var oldReset = MakeExistingReset(user.UserId, sentSecondsAgo: 90);

        _userDal
            .Setup(x => x.GetUserByEmail(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _passwordResetDal
            .Setup(x => x.GetPasswordReset(user.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(oldReset);

        var useCase = CreateUseCase(emailEnabled: true);

        // Act
        await useCase.ForgotPassword(user.Email!, CancellationToken.None);

        // Assert
        _passwordResetDal.Verify(
            x => x.ReplacePasswordReset(
                It.Is<PasswordResetRequestDal>(r =>
                    r.UserId == user.UserId &&
                    r.Attempts == 0 &&
                    r.ExpiresAt > DateTimeOffset.UtcNow),
                It.IsAny<CancellationToken>()),
            Times.Once);
        _emailService.Verify(
            x => x.SendPasswordResetAsync(user.Email!, It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // --- No existing reset record ---

    [Fact]
    public async Task ForgotPassword_WhenNoExistingReset_ShouldReplaceResetAndSendEmail()
    {
        // Arrange
        var user = MakeUser();

        _userDal
            .Setup(x => x.GetUserByEmail(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _passwordResetDal
            .Setup(x => x.GetPasswordReset(user.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PasswordResetDal?)null);

        var useCase = CreateUseCase(emailEnabled: true);

        // Act
        await useCase.ForgotPassword(user.Email!, CancellationToken.None);

        // Assert
        _passwordResetDal.Verify(
            x => x.ReplacePasswordReset(
                It.Is<PasswordResetRequestDal>(r =>
                    r.UserId == user.UserId &&
                    r.Attempts == 0 &&
                    r.ExpiresAt > DateTimeOffset.UtcNow),
                It.IsAny<CancellationToken>()),
            Times.Once);
        _emailService.Verify(
            x => x.SendPasswordResetAsync(user.Email!, It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // --- Email enabled/disabled ---

    [Fact]
    public async Task ForgotPassword_WhenEmailDisabled_ShouldReplaceResetButNotSendEmail()
    {
        // Arrange
        var user = MakeUser();

        _userDal
            .Setup(x => x.GetUserByEmail(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _passwordResetDal
            .Setup(x => x.GetPasswordReset(user.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PasswordResetDal?)null);

        var useCase = CreateUseCase(emailEnabled: false);

        // Act
        await useCase.ForgotPassword(user.Email!, CancellationToken.None);

        // Assert
        _passwordResetDal.Verify(
            x => x.ReplacePasswordReset(It.IsAny<PasswordResetRequestDal>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _emailService.Verify(
            x => x.SendPasswordResetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ForgotPassword_ShouldTrimEmailBeforeLookup()
    {
        // Arrange
        _userDal
            .Setup(x => x.GetUserByEmail("john@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((LoginUserDal?)null);

        var useCase = CreateUseCase();

        // Act
        await useCase.ForgotPassword("  john@example.com  ", CancellationToken.None);

        // Assert — lookup was called with trimmed email
        _userDal.Verify(
            x => x.GetUserByEmail("john@example.com", It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
