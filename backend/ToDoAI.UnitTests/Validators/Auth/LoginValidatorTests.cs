using ToDoAI.API.Controllers.Auth.Models;
using ToDoAI.API.Validators;

namespace ToDoAI.UnitTests.Validators.Auth;

public sealed class LoginValidatorTests
{
    private readonly LoginValidator _validator = new();

    private static LoginUserRequest ValidRequest(
        string userName = "john_doe1",
        string password = "Password1!") => new()
    {
        UserName = userName,
        Password = password
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

    // --- UserName ---

    [Fact]
    public void Validate_WhenUserNameIsEmpty_ShouldHaveError()
    {
        // Arrange
        var request = ValidRequest(userName: "");

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(request.UserName));
    }

    [Theory]
    [InlineData("ab")]
    [InlineData("short")]
    public void Validate_WhenUserNameIsTooShort_ShouldHaveError(string userName)
    {
        // Arrange
        var request = ValidRequest(userName: userName);

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(request.UserName));
    }

    [Fact]
    public void Validate_WhenUserNameExceedsMaxLength_ShouldHaveError()
    {
        // Arrange
        var request = ValidRequest(userName: new string('a', 101));

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(request.UserName));
    }

    // --- Password ---

    [Fact]
    public void Validate_WhenPasswordIsEmpty_ShouldHaveError()
    {
        // Arrange
        var request = ValidRequest(password: "");

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(request.Password));
    }

    [Fact]
    public void Validate_WhenPasswordIsTooShort_ShouldHaveError()
    {
        // Arrange
        var request = ValidRequest(password: "Ab1!");

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(request.Password));
    }

    [Fact]
    public void Validate_WhenPasswordHasNoLowercase_ShouldHaveError()
    {
        // Arrange
        var request = ValidRequest(password: "PASSWORD1!");

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(request.Password));
    }

    [Fact]
    public void Validate_WhenPasswordHasNoDigit_ShouldHaveError()
    {
        // Arrange
        var request = ValidRequest(password: "Password!!");

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(request.Password));
    }

    [Fact]
    public void Validate_WhenPasswordHasNoSpecialChar_ShouldHaveError()
    {
        // Arrange
        var request = ValidRequest(password: "Password123");

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(request.Password));
    }
}
