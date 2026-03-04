namespace ToDoAI.ToDoAI.Domain.Entities;

public sealed class ScheduleEntity
{
    public Guid Id { get; set; }
    
    public TaskEntity? Task { get; set; }
    
    public DateTimeOffset Start { get; set; }
    
    public DateTimeOffset End { get; set; }
}