using ToDoAI.API.Controllers.TaskController.Models;
using ToDoAI.API.Validators;

namespace ToDoAI.UnitTests.Validators.Tasks;

public sealed class TaskStatusValidatorTests
{
    private readonly TaskStatusValidator _validator = new();

    [Theory]
    [InlineData(TaskWorkStatusFilters.New)]
    [InlineData(TaskWorkStatusFilters.Todo)]
    [InlineData(TaskWorkStatusFilters.Running)]
    [InlineData(TaskWorkStatusFilters.Completed)]
    public void Validate_WhenWorkStatusIsDefinedEnumValue_ShouldNotHaveErrors(TaskWorkStatusFilters status)
    {
        // Arrange
        var request = new TaskStatusRequest { WorkStatus = status };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WhenWorkStatusIsUndefinedEnumValue_ShouldHaveError()
    {
        // Arrange
        var request = new TaskStatusRequest { WorkStatus = (TaskWorkStatusFilters)99 };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(request.WorkStatus));
    }
}
