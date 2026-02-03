namespace ToDoAI.ToDoAI.Domain.Entities;

public sealed class TaskEntity
{
    public Guid Id { get; set; }
    
    public int ComplexityLevel { get; set; }
    
    public int Priority { get; set; }
    
    public string Text { get; set; } = default!;
    
    public string Description { get; set; } = default!;
    
    public DateTimeOffset StartDate { get; set; }
    
    public DateTimeOffset EndDate { get; set; }
    
    public DateTimeOffset UpdatedDate { get; set; }
    
    public TaskStatus WorkStatus { get; set; } = default!;
    
    public Guid UserId { get; set; }
}