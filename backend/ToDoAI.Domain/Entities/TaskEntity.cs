using TaskStatus = ToDoAI.ToDoAI.Domain.Enums.TaskStatus;

namespace ToDoAI.ToDoAI.Domain.Entities;

public sealed class TaskEntity
{
    public Guid Id { get; init; }
    
    public string Title { get; init; } = default!;
    
    public string Description { get; init; } = default!;
    
    public int EstimatedMinutes { get; init; }
    
    public int ComplexityLevel { get; init; }
    
    public int Priority { get; init; }
    
    public DateTimeOffset CreatedAt { get; init; }
    
    public DateTimeOffset? ActualStartDate { get; init; }
    
    public DateTimeOffset? ActualEndDate { get; init; }
    
    public DateTimeOffset UpdatedAt { get; init; }
    
    public TaskStatus WorkStatus { get; init; }
    
    public Guid UserId { get; init; }

    public UserEntity User { get; init; } = default!;
}
