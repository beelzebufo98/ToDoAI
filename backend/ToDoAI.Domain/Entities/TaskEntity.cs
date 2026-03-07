namespace ToDoAI.ToDoAI.Domain.Entities;

public sealed class TaskEntity
{
    public Guid Id { get; set; }
    
    public string Title { get; set; } = default!;
    
    public string Description { get; set; } = default!;
    
    public int EstimatedMinutes { get; set; }
    
    public int ComplexityLevel { get; set; }
    
    public int Priority { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    
    public DateTimeOffset UpdatedAt { get; set; }
    
    public TaskStatus WorkStatus { get; set; } = default!;
    
    public Guid UserId { get; set; }

    public UserEntity User { get; set; } = default!;
}
