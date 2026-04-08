namespace ToDoAI.API.Controllers.TaskController.Models;

public sealed class CreateTaskRequest
{
    public required string Title { get; init; }
    
    public required string Description { get; init; }
    
    public required int EstimatedMinutes { get; init; }
    
    public required int ComplexityLevel { get; init; }
    
    public required int Priority { get; init; }
}