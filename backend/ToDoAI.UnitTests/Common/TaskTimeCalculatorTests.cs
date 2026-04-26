using ToDoAI.Application.Common;
using ToDoAI.Domain.Enums;

namespace ToDoAI.UnitTests.Common;

public sealed class TaskTimeCalculatorTests
{
    [Theory]
    [InlineData(60, 0)]
    [InlineData(60, 30)]
    [InlineData(60, 60)]
    [InlineData(60, 90)]
    public void CalculateRemainingMinutes_WhenStatusIsCompleted_ShouldReturnZero(
        int estimatedMinutes, int actualSpentMinutes)
    {
        // Act
        var result = TaskTimeCalculator.CalculateRemainingMinutes(
            estimatedMinutes, actualSpentMinutes, WorkStatus.Completed);

        // Assert
        result.Should().Be(0);
    }

    [Theory]
    [InlineData(60, 30, 30)]
    [InlineData(120, 0, 120)]
    [InlineData(120, 119, 1)]
    [InlineData(100, 50, 50)]
    public void CalculateRemainingMinutes_WhenActualLessThanEstimated_ShouldReturnDifference(
        int estimatedMinutes, int actualSpentMinutes, int expected)
    {
        // Act
        var result = TaskTimeCalculator.CalculateRemainingMinutes(
            estimatedMinutes, actualSpentMinutes, WorkStatus.Running);

        // Assert
        result.Should().Be(expected);
    }

    // estimated/3 < 30 → clamped to 30
    [Theory]
    [InlineData(60, 60)]   // 60/3=20 → max(20,30)=30 → min(30,60)=30
    [InlineData(30, 30)]   // 30/3=10 → 30
    [InlineData(10, 20)]   // 10/3=3  → 30
    public void CalculateRemainingMinutes_WhenOverrunAndEstimatedIsSmall_ShouldReturnMinimum30(
        int estimatedMinutes, int actualSpentMinutes)
    {
        // Act
        var result = TaskTimeCalculator.CalculateRemainingMinutes(
            estimatedMinutes, actualSpentMinutes, WorkStatus.Running);

        // Assert
        result.Should().Be(30);
    }

    // estimated/3 > 60 → clamped to 60
    [Theory]
    [InlineData(300, 350)]  // 300/3=100 → min(100,60)=60
    [InlineData(240, 250)]  // 240/3=80  → min(80,60)=60
    [InlineData(180, 200)]  // 180/3=60  → min(60,60)=60
    public void CalculateRemainingMinutes_WhenOverrunAndEstimatedIsLarge_ShouldReturnMaximum60(
        int estimatedMinutes, int actualSpentMinutes)
    {
        // Act
        var result = TaskTimeCalculator.CalculateRemainingMinutes(
            estimatedMinutes, actualSpentMinutes, WorkStatus.Running);

        // Assert
        result.Should().Be(60);
    }

    [Fact]
    public void CalculateRemainingMinutes_WhenActualLessThanEstimated_StatusIsNew_ShouldReturnDifference()
    {
        // Arrange
        const int estimatedMinutes = 90;
        const int actualSpentMinutes = 45;

        // Act
        var result = TaskTimeCalculator.CalculateRemainingMinutes(
            estimatedMinutes, actualSpentMinutes, WorkStatus.New);

        // Assert
        result.Should().Be(45);
    }

    [Fact]
    public void CalculateRemainingMinutes_Result_ShouldNeverBeNegative()
    {
        // Arrange
        const int estimatedMinutes = 60;
        const int actualSpentMinutes = 1000;

        // Act
        var result = TaskTimeCalculator.CalculateRemainingMinutes(
            estimatedMinutes, actualSpentMinutes, WorkStatus.Running);

        // Assert
        result.Should().BeGreaterThanOrEqualTo(0);
    }
}
