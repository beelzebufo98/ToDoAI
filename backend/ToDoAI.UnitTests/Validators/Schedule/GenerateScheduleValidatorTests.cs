using ToDoAI.API.Controllers.ScheduleController.Models;
using ToDoAI.API.Validators;

namespace ToDoAI.UnitTests.Validators.Schedule;

public sealed class GenerateScheduleValidatorTests
{
    private readonly GenerateScheduleValidator _validator = new();

    private static readonly DateOnly ValidDate = new(2026, 5, 1);
    private static readonly DateTimeOffset ValidStartAt = new(2026, 5, 1, 9, 0, 0, TimeSpan.Zero);

    private static GenerateScheduleRequest ValidRequest(
        DateOnly? scheduleDate = null,
        DateTimeOffset? startAt = null,
        IEnumerable<Guid>? taskIds = null) => new()
    {
        ScheduleDate = scheduleDate ?? ValidDate,
        StartAt = startAt ?? ValidStartAt,
        TaskIds = taskIds?.ToList() ?? [Guid.NewGuid()]
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

    // --- ScheduleDate ---

    [Fact]
    public void Validate_WhenScheduleDateIsDefault_ShouldHaveError()
    {
        // Arrange
        var request = new GenerateScheduleRequest
        {
            ScheduleDate = default,
            StartAt = ValidStartAt,
            TaskIds = [Guid.NewGuid()]
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(request.ScheduleDate));
    }

    // --- StartAt ---

    [Fact]
    public void Validate_WhenStartAtIsDefault_ShouldHaveError()
    {
        // Arrange
        var request = new GenerateScheduleRequest
        {
            ScheduleDate = ValidDate,
            StartAt = default,
            TaskIds = [Guid.NewGuid()]
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(request.StartAt));
    }

    // --- StartAt must match ScheduleDate ---

    [Fact]
    public void Validate_WhenStartAtDateDoesNotMatchScheduleDate_ShouldHaveError()
    {
        // Arrange
        var request = ValidRequest(
            scheduleDate: new DateOnly(2026, 5, 1),
            startAt: new DateTimeOffset(2026, 5, 2, 9, 0, 0, TimeSpan.Zero));

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Start time must belong to schedule date.");
    }

    [Fact]
    public void Validate_WhenStartAtDateMatchesScheduleDate_ShouldNotHaveCrossFieldError()
    {
        // Arrange
        var request = ValidRequest(
            scheduleDate: new DateOnly(2026, 5, 1),
            startAt: new DateTimeOffset(2026, 5, 1, 18, 30, 0, TimeSpan.Zero));

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.Errors.Should().NotContain(e => e.ErrorMessage == "Start time must belong to schedule date.");
    }

    // --- TaskIds ---

    [Fact]
    public void Validate_WhenTaskIdsIsEmpty_ShouldHaveError()
    {
        // Arrange
        var request = ValidRequest(taskIds: []);

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(request.TaskIds));
    }

    [Fact]
    public void Validate_WhenTaskIdsContainsDuplicates_ShouldHaveError()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var request = ValidRequest(taskIds: [taskId, taskId]);

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Task ids must be unique.");
    }

    [Fact]
    public void Validate_WhenTaskIdsAreUnique_ShouldNotHaveDuplicateError()
    {
        // Arrange
        var request = ValidRequest(taskIds: [Guid.NewGuid(), Guid.NewGuid()]);

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.Errors.Should().NotContain(e => e.ErrorMessage == "Task ids must be unique.");
    }
}
