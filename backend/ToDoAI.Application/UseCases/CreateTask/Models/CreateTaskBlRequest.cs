namespace ToDoAI.ToDoAI.Application.UseCases.CreateTask.Models;

public sealed record CreateTaskBlRequest
{
    public required string UserName { get; init; }
    
    public required string Title { get; init; }
    
    public required string Description { get; init; }
    
    public required int EstimatedMinutes { get; init; }
    
    public required int ComplexityLevel { get; init; }
    
    public required int Priority { get; init; }
}