namespace ToDoAI.Application.Abstractions.DalProviders.DayScheduleDalProvider.Models;

public sealed record ScheduleDalResult
{
    public Guid ScheduleId { get; set; }
    
    public Guid TaskId { get; set; }
    
    public DateTimeOffset StartAt { get; set; }
    
    public DateTimeOffset EndAt { get; set; }
    
    public string? TaskTitle { get; set; }
    
    public string? TaskDescription { get; set; }
}