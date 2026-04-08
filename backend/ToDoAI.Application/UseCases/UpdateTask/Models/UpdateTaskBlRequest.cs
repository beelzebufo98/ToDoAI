namespace ToDoAI.Application.UseCases.UpdateTask.Models;

public class UpdateTaskBlRequest
{
    public Guid TaskId { get; init; }
    
    public Guid UserId { get; init; }
    
    public string? Title { get; init; }
    
    public string? Description { get; init; }
    
    public int? EstimatedMinutes { get; init; }
    
    public int? ComplexityLevel { get; init; }
    
    public int? Priority { get; init; }
}