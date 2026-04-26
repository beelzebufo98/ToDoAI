using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ToDoAI.Application.Abstractions.DalProviders.EmailConfirmationDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.EmailConfirmationDalProvider.Models;
using ToDoAI.Application.Abstractions.DalProviders.UserDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.UserDalProvider.Models;
using ToDoAI.Application.Services.EmailService;
using ToDoAI.Application.Services.EmailService.Settings;
using ToDoAI.Application.UseCases.ResendConfirmationCode;

namespace ToDoAI.UnitTests.UseCases;

public sealed class ResendConfirmationCodeUseCaseTests
{
    private readonly Mock<IUserDalProvider> _userDal = new();
    private readonly Mock<IEmailConfirmationDalProvider> _confirmationDal = new();
    private readonly Mock<IEmailService> _emailService = new();
    private readonly Mock<ILogger<ResendConfirmationCodeUseCase>> _logger = new();

    private ResendConfirmationCodeUseCase CreateUseCase(bool emailEnabled = true) =>
        new(
            _userDal.Object,
            _confirmationDal.Object,
            _emailService.Object,
            Options.Create(new EmailSettings { Enabled = emailEnabled }),
            _logger.Object);

    private static LoginUserDal MakeUser(bool isEmailConfirmed = false) => new()
    {
        UserId = Guid.NewGuid(),
        UserName = "john_doe",
        FirstName = "John",
        Email = "john@example.com",
        PasswordHash = "hash",
        IsEmailConfirmed = isEmailConfirmed
    };

    private static EmailConfirmationDal MakeExistingConfirmation(Guid userId, int sentSecondsAgo) => new()
    {
        Id = Guid.NewGuid(),
        UserId = userId,
        CodeHash = "some_hash",
        ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(10),
        SentAt = DateTimeOffset.UtcNow.AddSeconds(-sentSecondsAgo),
        Attempts = 0
    };

    // --- Silent exits (no side effects) ---

    [Fact]
    public async Task ResendConfirmationCode_WhenUserNotFound_ShouldDoNothing()
    {
        // Arrange
        _userDal
            .Setup(x => x.GetUserByEmail(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((LoginUserDal?)null);

        var useCase = CreateUseCase();

        // Act
        await useCase.ResendConfirmationCode("ghost@example.com", CancellationToken.None);

        // Assert
        _confirmationDal.VerifyNoOtherCalls();
        _emailService.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ResendConfirmationCode_WhenEmailAlreadyConfirmed_ShouldDoNothing()
    {
        // Arrange
        var user = MakeUser(isEmailConfirmed: true);
        _userDal
            .Setup(x => x.GetUserByEmail(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var useCase = CreateUseCase();

        // Act
        await useCase.ResendConfirmationCode(user.Email!, CancellationToken.None);

        // Assert
        _confirmationDal.VerifyNoOtherCalls();
        _emailService.VerifyNoOtherCalls();
    }

    // --- Cooldown ---

    [Fact]
    public async Task ResendConfirmationCode_WhenWithinCooldown_ShouldDoNothing()
    {
        // Arrange
        var user = MakeUser();
        var recentConfirmation = MakeExistingConfirmation(user.UserId, sentSecondsAgo: 30);

        _userDal
            .Setup(x => x.GetUserByEmail(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _confirmationDal
            .Setup(x => x.GetEmailConfirmation(user.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(recentConfirmation);

        var useCase = CreateUseCase();

        // Act
        await useCase.ResendConfirmationCode(user.Email!, CancellationToken.None);

        // Assert
        _confirmationDal.Verify(
            x => x.ReplaceEmailConfirmation(It.IsAny<EmailConfirmationRequestDal>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _emailService.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ResendConfirmationCode_WhenCooldownExpired_ShouldReplaceConfirmationAndSendEmail()
    {
        // Arrange
        var user = MakeUser();
        var oldConfirmation = MakeExistingConfirmation(user.UserId, sentSecondsAgo: 90);

        _userDal
            .Setup(x => x.GetUserByEmail(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _confirmationDal
            .Setup(x => x.GetEmailConfirmation(user.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(oldConfirmation);

        var useCase = CreateUseCase(emailEnabled: true);

        // Act
        await useCase.ResendConfirmationCode(user.Email!, CancellationToken.None);

        // Assert
        _confirmationDal.Verify(
            x => x.ReplaceEmailConfirmation(
                It.Is<EmailConfirmationRequestDal>(r =>
                    r.UserId == user.UserId &&
                    r.Attempts == 0 &&
                    r.ExpiresAt > DateTimeOffset.UtcNow),
                It.IsAny<CancellationToken>()),
            Times.Once);
        _emailService.Verify(
            x => x.SendEmailConfirmationAsync(user.Email!, It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // --- No existing confirmation (first send after expiry/deletion) ---

    [Fact]
    public async Task ResendConfirmationCode_WhenNoExistingConfirmation_ShouldReplaceConfirmationAndSendEmail()
    {
        // Arrange
        var user = MakeUser();

        _userDal
            .Setup(x => x.GetUserByEmail(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _confirmationDal
            .Setup(x => x.GetEmailConfirmation(user.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((EmailConfirmationDal?)null);

        var useCase = CreateUseCase(emailEnabled: true);

        // Act
        await useCase.ResendConfirmationCode(user.Email!, CancellationToken.None);

        // Assert
        _confirmationDal.Verify(
            x => x.ReplaceEmailConfirmation(It.IsAny<EmailConfirmationRequestDal>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _emailService.Verify(
            x => x.SendEmailConfirmationAsync(user.Email!, It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // --- Email enabled/disabled ---

    [Fact]
    public async Task ResendConfirmationCode_WhenEmailDisabled_ShouldReplaceConfirmationButNotSendEmail()
    {
        // Arrange
        var user = MakeUser();

        _userDal
            .Setup(x => x.GetUserByEmail(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _confirmationDal
            .Setup(x => x.GetEmailConfirmation(user.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((EmailConfirmationDal?)null);

        var useCase = CreateUseCase(emailEnabled: false);

        // Act
        await useCase.ResendConfirmationCode(user.Email!, CancellationToken.None);

        // Assert
        _confirmationDal.Verify(
            x => x.ReplaceEmailConfirmation(It.IsAny<EmailConfirmationRequestDal>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _emailService.Verify(
            x => x.SendEmailConfirmationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
