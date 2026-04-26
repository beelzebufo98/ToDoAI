using ToDoAI.API.Controllers.TaskExecutionController.Models;
using ToDoAI.API.Validators;

namespace ToDoAI.UnitTests.Validators.TaskExecution;

public sealed class TaskExecutionValidatorTests
{
    private readonly TaskExecutionValidator _validator = new();

    private static CreateTaskExecutionRequest ValidRequest() => new()
    {
        ActualMinutes = 30,
        EnergyAfter = 6,
        StressAfter = 4
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

    // --- ActualMinutes ---

    [Fact]
    public void Validate_WhenActualMinutesIsZero_ShouldHaveError()
    {
        // Arrange
        var request = new CreateTaskExecutionRequest { ActualMinutes = 0, EnergyAfter = 6, StressAfter = 4 };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(request.ActualMinutes));
    }

    [Fact]
    public void Validate_WhenActualMinutesIsNegative_ShouldHaveError()
    {
        // Arrange
        var request = new CreateTaskExecutionRequest { ActualMinutes = -1, EnergyAfter = 6, StressAfter = 4 };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(request.ActualMinutes));
    }

    // --- EnergyAfter ---

    [Fact]
    public void Validate_WhenEnergyAfterIsZero_ShouldHaveError()
    {
        // Arrange
        var request = new CreateTaskExecutionRequest { ActualMinutes = 30, EnergyAfter = 0, StressAfter = 4 };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(request.EnergyAfter));
    }

    [Fact]
    public void Validate_WhenEnergyAfterExceedsTen_ShouldHaveError()
    {
        // Arrange
        var request = new CreateTaskExecutionRequest { ActualMinutes = 30, EnergyAfter = 11, StressAfter = 4 };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(request.EnergyAfter));
    }

    // --- StressAfter ---

    [Fact]
    public void Validate_WhenStressAfterIsZero_ShouldHaveError()
    {
        // Arrange
        var request = new CreateTaskExecutionRequest { ActualMinutes = 30, EnergyAfter = 6, StressAfter = 0 };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(request.StressAfter));
    }

    [Fact]
    public void Validate_WhenStressAfterExceedsTen_ShouldHaveError()
    {
        // Arrange
        var request = new CreateTaskExecutionRequest { ActualMinutes = 30, EnergyAfter = 6, StressAfter = 11 };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(request.StressAfter));
    }
}
