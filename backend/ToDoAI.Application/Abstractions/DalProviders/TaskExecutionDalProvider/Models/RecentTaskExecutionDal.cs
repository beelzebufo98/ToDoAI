namespace ToDoAI.Application.Abstractions.DalProviders.TaskExecutionDalProvider.Models;

public sealed record RecentTaskExecutionDal
{
    public Guid TaskExecutionId { get; init; }

    public Guid TaskId { get; init; }

    public string TaskTitle { get; init; } = default!;

    public int TaskEstimatedMinutes { get; init; }

    public int TaskComplexityLevel { get; init; }

    public int TaskPriority { get; init; }

    public int ActualMinutes { get; init; }

    public int EnergyAfter { get; init; }

    public int StressAfter { get; init; }

    public DateTimeOffset CreatedAt { get; init; }
}
