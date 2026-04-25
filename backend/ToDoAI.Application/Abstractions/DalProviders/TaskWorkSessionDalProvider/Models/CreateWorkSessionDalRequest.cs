namespace ToDoAI.Application.Abstractions.DalProviders.TaskWorkSessionDalProvider.Models;

public sealed record CreateWorkSessionDalRequest
{
    public Guid UserId { get; set; }
    
    public Guid TaskId { get; set; }
    
    public Guid? ScheduleId { get; set; }
}