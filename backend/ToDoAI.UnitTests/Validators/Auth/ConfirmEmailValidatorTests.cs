using ToDoAI.API.Controllers.Auth.Models;
using ToDoAI.API.Validators;

namespace ToDoAI.UnitTests.Validators.Auth;

public sealed class ConfirmEmailValidatorTests
{
    private readonly ConfirmEmailValidator _validator = new();

    private static ConfirmEmailRequest ValidRequest(
        string email = "user@example.com",
        string code = "123456") => new()
    {
        Email = email,
        Code = code
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

    [Theory]
    [InlineData("notanemail")]
    [InlineData("missing@")]
    [InlineData("@nodomain.com")]
    public void Validate_WhenEmailIsInvalidFormat_ShouldHaveError(string email)
    {
        // Arrange
        var request = ValidRequest(email: email);

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
    [InlineData("12345")]    // 5 digits
    [InlineData("1234567")]  // 7 digits
    [InlineData("1")]
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

    [Theory]
    [InlineData("abcdef")]
    [InlineData("12345a")]
    [InlineData("!23456")]
    public void Validate_WhenCodeContainsNonDigits_ShouldHaveError(string code)
    {
        // Arrange
        var request = ValidRequest(code: code);

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(request.Code));
    }

    [Theory]
    [InlineData("000000")]
    [InlineData("999999")]
    [InlineData("123456")]
    public void Validate_WhenCodeIsSixDigits_ShouldNotHaveError(string code)
    {
        // Arrange
        var request = ValidRequest(code: code);

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(request.Code));
    }
}
