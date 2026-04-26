using ToDoAI.API.Controllers.TaskController.Models;
using ToDoAI.API.Validators;

namespace ToDoAI.UnitTests.Validators.Tasks;

public sealed class TaskFiltersValidatorTests
{
    private readonly TaskFiltersValidator _validator = new();

    [Fact]
    public void Validate_WhenRequestHasDefaults_ShouldNotHaveErrors()
    {
        // Arrange
        var request = new TaskFiltersRequest();

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    // --- Page ---

    [Fact]
    public void Validate_WhenPageIsNull_ShouldNotHaveError()
    {
        // Arrange
        var request = new TaskFiltersRequest { Page = null };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(request.Page));
    }

    [Fact]
    public void Validate_WhenPageIsOne_ShouldNotHaveError()
    {
        // Arrange
        var request = new TaskFiltersRequest { Page = 1 };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(request.Page));
    }

    [Fact]
    public void Validate_WhenPageIsZero_ShouldHaveError()
    {
        // Arrange
        var request = new TaskFiltersRequest { Page = 0 };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(request.Page));
    }

    [Fact]
    public void Validate_WhenPageIsNegative_ShouldHaveError()
    {
        // Arrange
        var request = new TaskFiltersRequest { Page = -1 };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(request.Page));
    }

    // --- PageSize ---

    [Theory]
    [InlineData(1)]
    [InlineData(50)]
    [InlineData(100)]
    public void Validate_WhenPageSizeIsInRange_ShouldNotHaveError(int pageSize)
    {
        // Arrange
        var request = new TaskFiltersRequest { PageSize = pageSize };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == nameof(request.PageSize));
    }

    [Fact]
    public void Validate_WhenPageSizeIsZero_ShouldHaveError()
    {
        // Arrange
        var request = new TaskFiltersRequest { PageSize = 0 };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(request.PageSize));
    }

    [Fact]
    public void Validate_WhenPageSizeExceedsHundred_ShouldHaveError()
    {
        // Arrange
        var request = new TaskFiltersRequest { PageSize = 101 };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(request.PageSize));
    }
}
