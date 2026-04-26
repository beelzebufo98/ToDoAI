using Microsoft.Extensions.Logging;
using ToDoAI.Application.Abstractions.DalProviders.EmailConfirmationDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.EmailConfirmationDalProvider.Models;
using ToDoAI.Application.Abstractions.DalProviders.UserDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.UserDalProvider.Models;
using ToDoAI.Application.Common;
using ToDoAI.Application.UseCases.ConfirmEmail;
using ToDoAI.Application.UseCases.ConfirmEmail.Models;
using ToDoAI.Domain.Enums;

namespace ToDoAI.UnitTests.UseCases;

public sealed class ConfirmEmailUseCaseTests
{
    private readonly Mock<IUserDalProvider> _userDal = new();
    private readonly Mock<IEmailConfirmationDalProvider> _confirmationDal = new();

    private ConfirmEmailUseCase CreateUseCase() =>
        new(_userDal.Object, _confirmationDal.Object);

    private static LoginUserDal MakeUser(bool isEmailConfirmed = false) => new()
    {
        UserId = Guid.NewGuid(),
        UserName = "john_doe",
        FirstName = "John",
        Email = "user@example.com",
        PasswordHash = "hash",
        IsEmailConfirmed = isEmailConfirmed
    };

    private static EmailConfirmationDal MakeConfirmation(Guid userId, string code, bool expired = false) => new()
    {
        Id = Guid.NewGuid(),
        UserId = userId,
        CodeHash = ConfirmationCodeHelper.Hash(code),
        ExpiresAt = expired
            ? DateTimeOffset.UtcNow.AddMinutes(-1)
            : DateTimeOffset.UtcNow.AddMinutes(10),
        SentAt = DateTimeOffset.UtcNow.AddMinutes(-5),
        Attempts = 0
    };

    // --- User not found ---

    [Fact]
    public async Task ConfirmEmail_WhenUserNotFound_ShouldReturnIncorrectValue()
    {
        // Arrange
        _userDal
            .Setup(x => x.GetUserByEmail(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((LoginUserDal?)null);

        var useCase = CreateUseCase();
        var request = new ConfirmEmailBlRequest { Email = "ghost@example.com", Code = "123456" };

        // Act
        var result = await useCase.ConfirmEmail(request, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Should().Be(ErrorCodes.IncorrectValue);
        _confirmationDal.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ConfirmEmail_WhenEmailAlreadyConfirmed_ShouldReturnSuccessWithoutCheckingCode()
    {
        // Arrange
        var user = MakeUser(isEmailConfirmed: true);
        _userDal
            .Setup(x => x.GetUserByEmail(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var useCase = CreateUseCase();
        var request = new ConfirmEmailBlRequest { Email = "user@example.com", Code = "any" };

        // Act
        var result = await useCase.ConfirmEmail(request, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Error.Should().BeNull();
        _confirmationDal.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ConfirmEmail_WhenConfirmationNotFound_ShouldReturnIncorrectValue()
    {
        // Arrange
        var user = MakeUser();
        _userDal
            .Setup(x => x.GetUserByEmail(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _confirmationDal
            .Setup(x => x.GetEmailConfirmation(user.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((EmailConfirmationDal?)null);

        var useCase = CreateUseCase();
        var request = new ConfirmEmailBlRequest { Email = "user@example.com", Code = "123456" };

        // Act
        var result = await useCase.ConfirmEmail(request, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Should().Be(ErrorCodes.IncorrectValue);
    }

    [Fact]
    public async Task ConfirmEmail_WhenCodeExpired_ShouldDeleteConfirmationAndReturnIncorrectValue()
    {
        // Arrange
        var user = MakeUser();
        var confirmation = MakeConfirmation(user.UserId, "123456", expired: true);

        _userDal
            .Setup(x => x.GetUserByEmail(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _confirmationDal
            .Setup(x => x.GetEmailConfirmation(user.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(confirmation);

        var useCase = CreateUseCase();
        var request = new ConfirmEmailBlRequest { Email = "user@example.com", Code = "123456" };

        // Act
        var result = await useCase.ConfirmEmail(request, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Should().Be(ErrorCodes.IncorrectValue);
        _confirmationDal.Verify(
            x => x.DeleteEmailConfirmations(user.UserId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ConfirmEmail_WhenCodeIsWrong_ShouldRegisterFailedAttemptAndReturnIncorrectValue()
    {
        // Arrange
        var user = MakeUser();
        var confirmation = MakeConfirmation(user.UserId, "999999");

        _userDal
            .Setup(x => x.GetUserByEmail(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _confirmationDal
            .Setup(x => x.GetEmailConfirmation(user.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(confirmation);

        var useCase = CreateUseCase();
        var request = new ConfirmEmailBlRequest { Email = "user@example.com", Code = "123456" };

        // Act
        var result = await useCase.ConfirmEmail(request, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Should().Be(ErrorCodes.IncorrectValue);
        _confirmationDal.Verify(
            x => x.RegisterFailedAttempt(user.UserId, 5, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ConfirmEmail_WhenCodeIsValid_ShouldConfirmEmailAndDeleteConfirmationAndReturnSuccess()
    {
        // Arrange
        var code = "123456";
        var user = MakeUser();
        var confirmation = MakeConfirmation(user.UserId, code);

        _userDal
            .Setup(x => x.GetUserByEmail(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _confirmationDal
            .Setup(x => x.GetEmailConfirmation(user.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(confirmation);

        var useCase = CreateUseCase();
        var request = new ConfirmEmailBlRequest { Email = "user@example.com", Code = code };

        // Act
        var result = await useCase.ConfirmEmail(request, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Error.Should().BeNull();
        _userDal.Verify(
            x => x.UpdateEmailConfirmed(user.UserId, true, It.IsAny<CancellationToken>()),
            Times.Once);
        _confirmationDal.Verify(
            x => x.DeleteEmailConfirmations(user.UserId, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
