namespace ToDoAI.API.Controllers.TaskController.Models;

public sealed class UpdateTaskRequest
{
    public string? Title { get; init; }
    
    public string? Description { get; init; }
    
    public int? EstimatedMinutes { get; init; }
    
    public int? ComplexityLevel { get; init; }
    
    public int? Priority { get; init; }

    public DateTimeOffset? DeadlineAt { get; init; }
}
