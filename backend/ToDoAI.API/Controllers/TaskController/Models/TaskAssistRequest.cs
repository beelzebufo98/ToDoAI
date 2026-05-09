namespace ToDoAI.API.Controllers.TaskController.Models;

public sealed class TaskAssistRequest
{
    public required string Title { get; set; }
    
    public required string Description { get; set; }
    
    public required DateTimeOffset DeadlineAt { get; set; }
}