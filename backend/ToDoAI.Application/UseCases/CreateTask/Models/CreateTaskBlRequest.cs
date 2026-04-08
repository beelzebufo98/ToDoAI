namespace ToDoAI.Application.UseCases.CreateTask.Models;

public sealed record CreateTaskBlRequest
{
    public required Guid UserId { get; init; }
    
    public required string Title { get; init; }
    
    public required string Description { get; init; }
    
    public required int EstimatedMinutes { get; init; }
    
    public required int ComplexityLevel { get; init; }
    
    public required int Priority { get; init; }
}
