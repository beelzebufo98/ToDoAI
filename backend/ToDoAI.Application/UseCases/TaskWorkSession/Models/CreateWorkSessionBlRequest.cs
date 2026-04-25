namespace ToDoAI.Application.UseCases.TaskWorkSession.Models;

public sealed record CreateWorkSessionBlRequest
{
    public Guid UserId { get; set; }
    
    public Guid TaskId { get; set; }
    
    public Guid? ScheduleId { get; set; }
}