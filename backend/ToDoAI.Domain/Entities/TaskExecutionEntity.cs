namespace ToDoAI.ToDoAI.Domain.Entities;

public sealed class TaskExecutionEntity
{
    public Guid Id { get; init; }

    public Guid TaskId { get; init; }

    public TaskEntity Task { get; init; } = default!;
    
    public int ActualMinutes { get; init; }
    
    public int EnergyAfter { get; init; }
    
    public int StressAfter { get; init; }

    public DateTimeOffset CreatedAt { get; init; }
}
