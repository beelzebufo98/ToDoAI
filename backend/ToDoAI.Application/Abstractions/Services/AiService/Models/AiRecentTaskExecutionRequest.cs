namespace ToDoAI.Application.Abstractions.Services.AiService.Models;

public sealed record AiRecentTaskExecutionRequest
{
    public required Guid TaskId { get; init; }

    public required string TaskTitle { get; init; }

    public int EstimatedMinutes { get; init; }

    public int ActualMinutes { get; init; }

    public int Priority { get; init; }

    public int ComplexityLevel { get; init; }

    public int EnergyAfter { get; init; }

    public int StressAfter { get; init; }

    public DateTimeOffset CreatedAt { get; init; }
}