using ToDoAI.API.Controllers.UserStateController.Models;
using ToDoAI.API.Validators;

namespace ToDoAI.UnitTests.Validators.UserState;

public sealed class CreateUserStateValidatorTests
{
    private readonly CreateUserStateValidator _validator = new();

    private static CreateUserStateRequest ValidRequest(
        int sleepMinutes = 480,
        int energyLevel = 7,
        int stressLevel = 3,
        int motivationLevel = 8,
        int concentrationLevel = 5) => new()
    {
        SleepMinutes = sleepMinutes,
        EnergyLevel = energyLevel,
        StressLevel = stressLevel,
        MotivationLevel = motivationLevel,
        ConcentrationLevel = concentrationLevel
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

    // --- SleepMinutes ---

    [Fact]
    public void Validate_WhenSleepMinutesIsZero_ShouldNotHaveError()
    {
        // Arrange
        var request = ValidRequest(sleepMinutes: 0);

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(request.SleepMinutes));
    }

    [Fact]
    public void Validate_WhenSleepMinutesIs1440_ShouldNotHaveError()
    {
        // Arrange
        var request = ValidRequest(sleepMinutes: 1440);

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(request.SleepMinutes));
    }

    [Fact]
    public void Validate_WhenSleepMinutesIsNegative_ShouldHaveError()
    {
        // Arrange
        var request = ValidRequest(sleepMinutes: -1);

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(request.SleepMinutes));
    }

    [Fact]
    public void Validate_WhenSleepMinutesExceeds1440_ShouldHaveError()
    {
        // Arrange
        var request = ValidRequest(sleepMinutes: 1441);

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(request.SleepMinutes));
    }

    // --- EnergyLevel ---

    [Fact]
    public void Validate_WhenEnergyLevelIsZero_ShouldHaveError()
    {
        // Arrange
        var request = ValidRequest(energyLevel: 0);

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(request.EnergyLevel));
    }

    [Fact]
    public void Validate_WhenEnergyLevelExceedsTen_ShouldHaveError()
    {
        // Arrange
        var request = ValidRequest(energyLevel: 11);

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(request.EnergyLevel));
    }

    // --- StressLevel ---

    [Fact]
    public void Validate_WhenStressLevelIsZero_ShouldHaveError()
    {
        // Arrange
        var request = ValidRequest(stressLevel: 0);

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(request.StressLevel));
    }

    [Fact]
    public void Validate_WhenStressLevelExceedsTen_ShouldHaveError()
    {
        // Arrange
        var request = ValidRequest(stressLevel: 11);

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(request.StressLevel));
    }

    // --- MotivationLevel ---

    [Fact]
    public void Validate_WhenMotivationLevelIsZero_ShouldHaveError()
    {
        // Arrange
        var request = ValidRequest(motivationLevel: 0);

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(request.MotivationLevel));
    }

    [Fact]
    public void Validate_WhenMotivationLevelExceedsTen_ShouldHaveError()
    {
        // Arrange
        var request = ValidRequest(motivationLevel: 11);

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(request.MotivationLevel));
    }

    // --- ConcentrationLevel ---

    [Fact]
    public void Validate_WhenConcentrationLevelIsZero_ShouldHaveError()
    {
        // Arrange
        var request = ValidRequest(concentrationLevel: 0);

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(request.ConcentrationLevel));
    }

    [Fact]
    public void Validate_WhenConcentrationLevelExceedsTen_ShouldHaveError()
    {
        // Arrange
        var request = ValidRequest(concentrationLevel: 11);

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(request.ConcentrationLevel));
    }
}
