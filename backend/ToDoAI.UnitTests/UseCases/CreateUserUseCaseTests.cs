using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ToDoAI.Application.Abstractions.DalProviders.EmailConfirmationDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.EmailConfirmationDalProvider.Models;
using ToDoAI.Application.Abstractions.DalProviders.UserDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.UserDalProvider.Models;
using ToDoAI.Application.Services.EmailService;
using ToDoAI.Application.Services.EmailService.Settings;
using ToDoAI.Application.UseCases.CreateUser;
using ToDoAI.Application.UseCases.CreateUser.Models;
using ToDoAI.Domain.Enums;

namespace ToDoAI.UnitTests.UseCases;

public sealed class CreateUserUseCaseTests
{
    private readonly Mock<IUserDalProvider> _userDal = new();
    private readonly Mock<IEmailConfirmationDalProvider> _confirmationDal = new();
    private readonly Mock<IEmailService> _emailService = new();
    private readonly Mock<ILogger<CreateUserUseCase>> _logger = new();

    private CreateUserUseCase CreateUseCase(bool emailEnabled = true) =>
        new(
            _userDal.Object,
            _confirmationDal.Object,
            _emailService.Object,
            Options.Create(new EmailSettings { Enabled = emailEnabled }),
            _logger.Object);

    private static RegisterUserBlRequest ValidRequest() => new()
    {
        UserName = "john_doe",
        FirstName = "John",
        LastName = "Doe",
        Email = "john@example.com",
        Password = "Password1!"
    };

    // --- Conflict checks ---

    [Fact]
    public async Task CreateUser_WhenUsernameAlreadyExists_ShouldReturnUserExists()
    {
        // Arrange
        _userDal
            .Setup(x => x.CheckUserExists("john_doe", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _userDal
            .Setup(x => x.CheckUserEmailExists(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var useCase = CreateUseCase();

        // Act
        var result = await useCase.CreateUser(ValidRequest(), CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Should().Be(ErrorCodes.UserExists);
        _userDal.Verify(x => x.CreateUser(It.IsAny<RegisterUserRequestDal>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateUser_WhenEmailAlreadyExists_ShouldReturnUserExists()
    {
        // Arrange
        _userDal
            .Setup(x => x.CheckUserExists(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _userDal
            .Setup(x => x.CheckUserEmailExists("john@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var useCase = CreateUseCase();

        // Act
        var result = await useCase.CreateUser(ValidRequest(), CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Should().Be(ErrorCodes.UserExists);
        _userDal.Verify(x => x.CreateUser(It.IsAny<RegisterUserRequestDal>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // --- Successful registration ---

    [Fact]
    public async Task CreateUser_WhenEmailEnabled_ShouldCreateUserAndSendConfirmationEmail()
    {
        // Arrange
        _userDal
            .Setup(x => x.CheckUserExists(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _userDal
            .Setup(x => x.CheckUserEmailExists(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var useCase = CreateUseCase(emailEnabled: true);

        // Act
        var result = await useCase.CreateUser(ValidRequest(), CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Error.Should().BeNull();

        _userDal.Verify(
            x => x.CreateUser(
                It.Is<RegisterUserRequestDal>(r =>
                    r.UserName == "john_doe" &&
                    r.Email == "john@example.com"),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _confirmationDal.Verify(
            x => x.ReplaceEmailConfirmation(
                It.Is<EmailConfirmationRequestDal>(r =>
                    r.UserId != Guid.Empty &&
                    r.Attempts == 0 &&
                    r.ExpiresAt > DateTimeOffset.UtcNow),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _emailService.Verify(
            x => x.SendEmailConfirmationAsync("john@example.com", It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateUser_WhenEmailDisabled_ShouldCreateUserAndNotSendEmail()
    {
        // Arrange
        _userDal
            .Setup(x => x.CheckUserExists(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _userDal
            .Setup(x => x.CheckUserEmailExists(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var useCase = CreateUseCase(emailEnabled: false);

        // Act
        var result = await useCase.CreateUser(ValidRequest(), CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();

        _userDal.Verify(
            x => x.CreateUser(It.IsAny<RegisterUserRequestDal>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _emailService.Verify(
            x => x.SendEmailConfirmationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateUser_ShouldTrimEmailBeforeUsing()
    {
        // Arrange
        _userDal
            .Setup(x => x.CheckUserExists(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _userDal
            .Setup(x => x.CheckUserEmailExists(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var request = ValidRequest() with { Email = "  john@example.com  " };
        var useCase = CreateUseCase(emailEnabled: false);

        // Act
        await useCase.CreateUser(request, CancellationToken.None);

        // Assert
        _userDal.Verify(
            x => x.CreateUser(
                It.Is<RegisterUserRequestDal>(r => r.Email == "john@example.com"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
