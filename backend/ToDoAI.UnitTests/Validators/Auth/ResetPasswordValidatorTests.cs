using ToDoAI.API.Controllers.Auth.Models;
using ToDoAI.API.Validators;

namespace ToDoAI.UnitTests.Validators.Auth;

public sealed class ResetPasswordValidatorTests
{
    private readonly ResetPasswordValidator _validator = new();

    private static ResetPasswordRequest ValidRequest(
        string email = "user@example.com",
        string code = "123456",
        string newPassword = "Password1!") => new()
    {
        Email = email,
        Code = code,
        NewPassword = newPassword
    };

    [Fact]
    public void Validate_WhenRequestIsValid_ShouldNotHaveErrors()
    {
        // Arrange
        var request = ValidRequest();

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    // --- Email ---

    [Fact]
    public void Validate_WhenEmailIsEmpty_ShouldHaveError()
    {
        // Arrange
        var request = ValidRequest(email: "");

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(request.Email));
    }

    [Fact]
    public void Validate_WhenEmailIsInvalidFormat_ShouldHaveError()
    {
        // Arrange
        var request = ValidRequest(email: "notanemail");

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(request.Email));
    }

    // --- Code ---

    [Fact]
    public void Validate_WhenCodeIsEmpty_ShouldHaveError()
    {
        // Arrange
        var request = ValidRequest(code: "");

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(request.Code));
    }

    [Theory]
    [InlineData("12345")]
    [InlineData("1234567")]
    public void Validate_WhenCodeLengthIsNot6_ShouldHaveError(string code)
    {
        // Arrange
        var request = ValidRequest(code: code);

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(request.Code));
    }

    [Fact]
    public void Validate_WhenCodeContainsLetters_ShouldHaveError()
    {
        // Arrange
        var request = ValidRequest(code: "12345a");

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(request.Code));
    }

    // --- NewPassword ---

    [Fact]
    public void Validate_WhenNewPasswordIsEmpty_ShouldHaveError()
    {
        // Arrange
        var request = ValidRequest(newPassword: "");

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(request.NewPassword));
    }

    [Fact]
    public void Validate_WhenNewPasswordIsTooShort_ShouldHaveError()
    {
        // Arrange
        var request = ValidRequest(newPassword: "Ab1!");

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(request.NewPassword));
    }

    [Fact]
    public void Validate_WhenNewPasswordHasNoLowercase_ShouldHaveError()
    {
        // Arrange
        var request = ValidRequest(newPassword: "PASSWORD1!");

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(request.NewPassword));
    }

    [Fact]
    public void Validate_WhenNewPasswordHasNoDigit_ShouldHaveError()
    {
        // Arrange
        var request = ValidRequest(newPassword: "Password!!");

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(request.NewPassword));
    }

    [Fact]
    public void Validate_WhenNewPasswordHasNoSpecialChar_ShouldHaveError()
    {
        // Arrange
        var request = ValidRequest(newPassword: "Password123");

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(request.NewPassword));
    }
}
