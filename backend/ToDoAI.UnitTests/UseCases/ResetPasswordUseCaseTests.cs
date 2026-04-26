using ToDoAI.Application.Abstractions.DalProviders.PasswordResetDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.PasswordResetDalProvider.Models;
using ToDoAI.Application.Abstractions.DalProviders.RefreshTokenDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.UserDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.UserDalProvider.Models;
using ToDoAI.Application.Common;
using ToDoAI.Application.UseCases.ResetPassword;
using ToDoAI.Application.UseCases.ResetPassword.Models;
using ToDoAI.Domain.Enums;

namespace ToDoAI.UnitTests.UseCases;

public sealed class ResetPasswordUseCaseTests
{
    private readonly Mock<IUserDalProvider> _userDal = new();
    private readonly Mock<IPasswordResetDalProvider> _passwordResetDal = new();
    private readonly Mock<IRefreshTokenDalProvider> _refreshTokenDal = new();

    private ResetPasswordUseCase CreateUseCase() =>
        new(_userDal.Object, _passwordResetDal.Object, _refreshTokenDal.Object);

    private static LoginUserDal MakeUser() => new()
    {
        UserId = Guid.NewGuid(),
        UserName = "john_doe",
        FirstName = "John",
        PasswordHash = "old_hash"
    };

    private static PasswordResetDal MakePasswordReset(Guid userId, string code, bool expired = false) => new()
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
    public async Task ResetPassword_WhenUserNotFound_ShouldReturnIncorrectValue()
    {
        // Arrange
        _userDal
            .Setup(x => x.GetUserByEmail(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((LoginUserDal?)null);

        var useCase = CreateUseCase();
        var request = new ResetPasswordBlRequest
        {
            Email = "ghost@example.com",
            Code = "123456",
            NewPassword = "NewPass1!"
        };

        // Act
        var result = await useCase.ResetPassword(request, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Should().Be(ErrorCodes.IncorrectValue);
        _passwordResetDal.VerifyNoOtherCalls();
    }

    // --- Reset record not found ---

    [Fact]
    public async Task ResetPassword_WhenPasswordResetNotFound_ShouldReturnIncorrectValue()
    {
        // Arrange
        var user = MakeUser();
        _userDal
            .Setup(x => x.GetUserByEmail(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _passwordResetDal
            .Setup(x => x.GetPasswordReset(user.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PasswordResetDal?)null);

        var useCase = CreateUseCase();
        var request = new ResetPasswordBlRequest
        {
            Email = "user@example.com",
            Code = "123456",
            NewPassword = "NewPass1!"
        };

        // Act
        var result = await useCase.ResetPassword(request, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Should().Be(ErrorCodes.IncorrectValue);
    }

    // --- Code expired ---

    [Fact]
    public async Task ResetPassword_WhenCodeExpired_ShouldDeleteResetsAndReturnIncorrectValue()
    {
        // Arrange
        var user = MakeUser();
        var passwordReset = MakePasswordReset(user.UserId, "123456", expired: true);

        _userDal
            .Setup(x => x.GetUserByEmail(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _passwordResetDal
            .Setup(x => x.GetPasswordReset(user.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(passwordReset);

        var useCase = CreateUseCase();
        var request = new ResetPasswordBlRequest
        {
            Email = "user@example.com",
            Code = "123456",
            NewPassword = "NewPass1!"
        };

        // Act
        var result = await useCase.ResetPassword(request, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Should().Be(ErrorCodes.IncorrectValue);
        _passwordResetDal.Verify(
            x => x.DeletePasswordResets(user.UserId, It.IsAny<CancellationToken>()),
            Times.Once);
        _userDal.Verify(
            x => x.UpdatePassword(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // --- Wrong code ---

    [Fact]
    public async Task ResetPassword_WhenCodeIsWrong_ShouldRegisterFailedAttemptAndReturnIncorrectValue()
    {
        // Arrange
        var user = MakeUser();
        var passwordReset = MakePasswordReset(user.UserId, "999999");

        _userDal
            .Setup(x => x.GetUserByEmail(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _passwordResetDal
            .Setup(x => x.GetPasswordReset(user.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(passwordReset);

        var useCase = CreateUseCase();
        var request = new ResetPasswordBlRequest
        {
            Email = "user@example.com",
            Code = "123456",
            NewPassword = "NewPass1!"
        };

        // Act
        var result = await useCase.ResetPassword(request, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Should().Be(ErrorCodes.IncorrectValue);
        _passwordResetDal.Verify(
            x => x.RegisterFailedAttempt(user.UserId, 5, It.IsAny<CancellationToken>()),
            Times.Once);
        _userDal.Verify(
            x => x.UpdatePassword(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // --- Valid code ---

    [Fact]
    public async Task ResetPassword_WhenCodeIsValid_ShouldUpdatePasswordAndInvalidateAllTokensAndReturnSuccess()
    {
        // Arrange
        var code = "123456";
        var user = MakeUser();
        var passwordReset = MakePasswordReset(user.UserId, code);

        _userDal
            .Setup(x => x.GetUserByEmail(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _passwordResetDal
            .Setup(x => x.GetPasswordReset(user.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(passwordReset);

        var useCase = CreateUseCase();
        var request = new ResetPasswordBlRequest
        {
            Email = "user@example.com",
            Code = code,
            NewPassword = "NewPass1!"
        };

        // Act
        var result = await useCase.ResetPassword(request, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Error.Should().BeNull();

        _userDal.Verify(
            x => x.UpdatePassword(user.UserId, It.IsNotNull<string>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _passwordResetDal.Verify(
            x => x.DeletePasswordResets(user.UserId, It.IsAny<CancellationToken>()),
            Times.Once);
        _refreshTokenDal.Verify(
            x => x.DeleteRefreshTokens(user.UserId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ResetPassword_WhenCodeIsValid_ShouldStoreNewHashNotPlainPassword()
    {
        // Arrange
        var code = "123456";
        var user = MakeUser();
        var passwordReset = MakePasswordReset(user.UserId, code);
        var plainPassword = "NewPass1!";

        _userDal
            .Setup(x => x.GetUserByEmail(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _passwordResetDal
            .Setup(x => x.GetPasswordReset(user.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(passwordReset);

        var useCase = CreateUseCase();
        var request = new ResetPasswordBlRequest
        {
            Email = "user@example.com",
            Code = code,
            NewPassword = plainPassword
        };

        // Act
        await useCase.ResetPassword(request, CancellationToken.None);

        // Assert
        _userDal.Verify(
            x => x.UpdatePassword(
                user.UserId,
                It.Is<string>(hash => hash != plainPassword && hash.Length > 0),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
