using ToDoAI.API.Controllers.Auth.Models;
using ToDoAI.API.Validators;

namespace ToDoAI.UnitTests.Validators.Auth;

public sealed class ForgotPasswordValidatorTests
{
    private readonly ForgotPasswordValidator _validator = new();

    [Fact]
    public void Validate_WhenEmailIsValid_ShouldNotHaveErrors()
    {
        // Arrange
        var request = new ForgotPasswordRequest { Email = "user@example.com" };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WhenEmailIsEmpty_ShouldHaveError()
    {
        // Arrange
        var request = new ForgotPasswordRequest { Email = "" };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(request.Email));
    }

    [Theory]
    [InlineData("notanemail")]
    [InlineData("user@@example.com")]
    [InlineData("@nodomain.com")]
    public void Validate_WhenEmailIsInvalidFormat_ShouldHaveError(string email)
    {
        // Arrange
        var request = new ForgotPasswordRequest { Email = email };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(request.Email));
    }

    [Fact]
    public void Validate_WhenEmailExceedsMaxLength_ShouldHaveError()
    {
        // Arrange
        var request = new ForgotPasswordRequest { Email = new string('a', 252) + "@b.co" };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(request.Email));
    }
}
