namespace ToDoAI.API.Controllers.TaskWorkSessionController.Models;

public sealed class StartWorkSessionRequest
{
    public Guid TaskId { get; set; }
    
    public Guid? ScheduleId { get; set; }
}