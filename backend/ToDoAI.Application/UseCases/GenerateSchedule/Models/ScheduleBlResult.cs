namespace ToDoAI.Application.UseCases.GenerateSchedule.Models;

public sealed record ScheduleBlResult
{
    public Guid ScheduleId { get; set; }
    
    public Guid TaskId { get; set; }
    
    public DateTimeOffset StartAt { get; set; }
    
    public DateTimeOffset EndAt { get; set; }
    
    public string? TaskTitle { get; set; }
    
    public string? TaskDescription { get; set; }
}