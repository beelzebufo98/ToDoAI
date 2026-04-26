using ToDoAI.API.Controllers.TaskController.Models;
using ToDoAI.API.Validators;

namespace ToDoAI.UnitTests.Validators.Tasks;

public sealed class UpdateTaskValidatorTests
{
    private readonly UpdateTaskValidator _validator = new();

    [Fact]
    public void Validate_WhenAllFieldsAreNull_ShouldNotHaveErrors()
    {
        // Arrange
        var request = new UpdateTaskRequest();

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    // --- Title ---

    [Fact]
    public void Validate_WhenTitleIsNotNullButEmpty_ShouldHaveError()
    {
        // Arrange
        var request = new UpdateTaskRequest { Title = "" };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(request.Title));
    }

    [Fact]
    public void Validate_WhenTitleIsNull_ShouldNotHaveError()
    {
        // Arrange
        var request = new UpdateTaskRequest { Title = null };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(request.Title));
    }

    // --- Description ---

    [Fact]
    public void Validate_WhenDescriptionIsNotNullButEmpty_ShouldHaveError()
    {
        // Arrange
        var request = new UpdateTaskRequest { Description = "" };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(request.Description));
    }

    [Fact]
    public void Validate_WhenDescriptionIsNull_ShouldNotHaveError()
    {
        // Arrange
        var request = new UpdateTaskRequest { Description = null };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(request.Description));
    }

    // --- EstimatedMinutes ---

    [Fact]
    public void Validate_WhenEstimatedMinutesIsZero_ShouldHaveError()
    {
        // Arrange
        var request = new UpdateTaskRequest { EstimatedMinutes = 0 };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(request.EstimatedMinutes));
    }

    [Fact]
    public void Validate_WhenEstimatedMinutesIsNegative_ShouldHaveError()
    {
        // Arrange
        var request = new UpdateTaskRequest { EstimatedMinutes = -5 };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(request.EstimatedMinutes));
    }

    [Fact]
    public void Validate_WhenEstimatedMinutesIsNull_ShouldNotHaveError()
    {
        // Arrange
        var request = new UpdateTaskRequest { EstimatedMinutes = null };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(request.EstimatedMinutes));
    }

    // --- ComplexityLevel ---

    [Fact]
    public void Validate_WhenComplexityLevelIsZero_ShouldHaveError()
    {
        // Arrange
        var request = new UpdateTaskRequest { ComplexityLevel = 0 };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(request.ComplexityLevel));
    }

    [Fact]
    public void Validate_WhenComplexityLevelExceedsTen_ShouldHaveError()
    {
        // Arrange
        var request = new UpdateTaskRequest { ComplexityLevel = 11 };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(request.ComplexityLevel));
    }

    [Fact]
    public void Validate_WhenComplexityLevelIsNull_ShouldNotHaveError()
    {
        // Arrange
        var request = new UpdateTaskRequest { ComplexityLevel = null };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(request.ComplexityLevel));
    }

    // --- Priority ---

    [Fact]
    public void Validate_WhenPriorityIsZero_ShouldHaveError()
    {
        // Arrange
        var request = new UpdateTaskRequest { Priority = 0 };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(request.Priority));
    }

    [Fact]
    public void Validate_WhenPriorityExceedsTen_ShouldHaveError()
    {
        // Arrange
        var request = new UpdateTaskRequest { Priority = 11 };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(request.Priority));
    }

    [Fact]
    public void Validate_WhenPriorityIsNull_ShouldNotHaveError()
    {
        // Arrange
        var request = new UpdateTaskRequest { Priority = null };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(request.Priority));
    }

    // --- DeadlineAt ---

    [Fact]
    public void Validate_WhenDeadlineAtIsDefault_ShouldHaveError()
    {
        // Arrange
        var request = new UpdateTaskRequest { DeadlineAt = default(DateTimeOffset) };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(request.DeadlineAt));
    }

    [Fact]
    public void Validate_WhenDeadlineAtIsNull_ShouldNotHaveError()
    {
        // Arrange
        var request = new UpdateTaskRequest { DeadlineAt = null };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(request.DeadlineAt));
    }
}
