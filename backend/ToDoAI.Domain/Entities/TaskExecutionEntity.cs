namespace ToDoAI.ToDoAI.Domain.Entities;

public sealed class TaskExecutionEntity
{
    public Guid Id { get; set; }
    
    public TaskEntity Task { get; set; } = default!;

    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset EndTime { get; set; }

    public int ActualMinutes { get; set; }
    public int EnergyAfter { get; set; }
    public int StressAfter { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}