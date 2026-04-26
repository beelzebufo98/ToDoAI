using ToDoAI.API.Controllers.Auth.Models;
using ToDoAI.API.Validators;

namespace ToDoAI.UnitTests.Validators.Auth;

public sealed class AuthValidatorTests
{
    private readonly AuthValidator _validator = new();

    private static RegisterUserRequest ValidRequest(
        string userName = "john_doe1",
        string firstName = "John",
        string? lastName = null,
        string email = "john@example.com",
        string password = "Password1!") => new()
    {
        UserName = userName,
        FirstName = firstName,
        LastName = lastName,
        Email = email,
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
    [InlineData("abc")]
    [InlineData("abcde")]
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

    [Theory]
    [InlineData("user name")]
    [InlineData("user@name")]
    [InlineData("user-name")]
    [InlineData("user.name")]
    public void Validate_WhenUserNameContainsInvalidChars_ShouldHaveError(string userName)
    {
        // Arrange
        var request = ValidRequest(userName: userName);

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(request.UserName));
    }

    // --- FirstName ---

    [Fact]
    public void Validate_WhenFirstNameIsEmpty_ShouldHaveError()
    {
        // Arrange
        var request = ValidRequest(firstName: "");

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(request.FirstName));
    }

    [Theory]
    [InlineData("John1")]
    [InlineData("Jo hn")]
    [InlineData("123")]
    public void Validate_WhenFirstNameContainsNonLetters_ShouldHaveError(string firstName)
    {
        // Arrange
        var request = ValidRequest(firstName: firstName);

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(request.FirstName));
    }

    [Theory]
    [InlineData("Иван")]
    [InlineData("John")]
    [InlineData("Mary")]
    public void Validate_WhenFirstNameIsValidLettersOnly_ShouldNotHaveError(string firstName)
    {
        // Arrange
        var request = ValidRequest(firstName: firstName);

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(request.FirstName));
    }

    // --- LastName (optional) ---

    [Fact]
    public void Validate_WhenLastNameIsNull_ShouldNotHaveError()
    {
        // Arrange
        var request = ValidRequest(lastName: null);

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(request.LastName));
    }

    [Fact]
    public void Validate_WhenLastNameContainsDigits_ShouldHaveError()
    {
        // Arrange
        var request = ValidRequest(lastName: "Smith1");

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(request.LastName));
    }

    [Fact]
    public void Validate_WhenLastNameExceedsMaxLength_ShouldHaveError()
    {
        // Arrange
        var request = ValidRequest(lastName: new string('A', 101));

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(request.LastName));
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
    [InlineData("user@@example.com")]
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

    [Fact]
    public void Validate_WhenEmailExceedsMaxLength_ShouldHaveError()
    {
        // Arrange
        var request = ValidRequest(email: new string('a', 252) + "@b.co");

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(request.Email));
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
    public void Validate_WhenPasswordExceedsMaxLength_ShouldHaveError()
    {
        // Arrange
        var request = ValidRequest(password: new string('a', 63) + "1!");

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
