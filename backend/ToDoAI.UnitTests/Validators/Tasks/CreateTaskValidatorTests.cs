using ToDoAI.API.Controllers.TaskController.Models;
using ToDoAI.API.Validators;

namespace ToDoAI.UnitTests.Validators.Tasks;

public sealed class CreateTaskValidatorTests
{
    private readonly CreateTaskValidator _validator = new();

    private static CreateTaskRequest ValidRequest(
        string title = "Task title",
        string description = "Task description",
        int estimatedMinutes = 30,
        int complexityLevel = 5,
        int priority = 5,
        DateTimeOffset? deadlineAt = null) => new()
    {
        Title = title,
        Description = description,
        EstimatedMinutes = estimatedMinutes,
        ComplexityLevel = complexityLevel,
        Priority = priority,
        DeadlineAt = deadlineAt ?? DateTimeOffset.UtcNow.AddDays(7)
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

    // --- Title ---

    [Fact]
    public void Validate_WhenTitleIsEmpty_ShouldHaveError()
    {
        // Arrange
        var request = ValidRequest(title: "");

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(request.Title));
    }

    // --- Description ---

    [Fact]
    public void Validate_WhenDescriptionIsEmpty_ShouldHaveError()
    {
        // Arrange
        var request = ValidRequest(description: "");

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(request.Description));
    }

    // --- EstimatedMinutes ---

    [Fact]
    public void Validate_WhenEstimatedMinutesIsZero_ShouldHaveError()
    {
        // Arrange
        var request = ValidRequest(estimatedMinutes: 0);

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
        var request = ValidRequest(estimatedMinutes: -1);

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(request.EstimatedMinutes));
    }

    // --- ComplexityLevel ---

    [Fact]
    public void Validate_WhenComplexityLevelIsZero_ShouldHaveError()
    {
        // Arrange
        var request = ValidRequest(complexityLevel: 0);

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
        var request = ValidRequest(complexityLevel: 11);

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(request.ComplexityLevel));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    public void Validate_WhenComplexityLevelIsInRange_ShouldNotHaveError(int level)
    {
        // Arrange
        var request = ValidRequest(complexityLevel: level);

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
        var request = ValidRequest(priority: 0);

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
        var request = ValidRequest(priority: 11);

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(request.Priority));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    public void Validate_WhenPriorityIsInRange_ShouldNotHaveError(int priority)
    {
        // Arrange
        var request = ValidRequest(priority: priority);

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
        var request = new CreateTaskRequest
        {
            Title = "Task title",
            Description = "Task description",
            EstimatedMinutes = 30,
            ComplexityLevel = 5,
            Priority = 5,
            DeadlineAt = default
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(request.DeadlineAt));
    }
}
