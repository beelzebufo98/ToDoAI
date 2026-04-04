using ToDoAI.ToDoAI.Domain.Enums;

namespace ToDoAI.ToDoAI.API.Controllers.TaskController.Models;

public sealed class GetTaskResponse
{
    public Guid Id { get; init; }
    
    public string Title { get; init; } = default!;
    
    public string Description { get; init; } = default!;
    
    public int EstimatedMinutes { get; init; }
    
    public int ComplexityLevel { get; init; }
    
    public int Priority { get; init; }
    
    public DateTimeOffset CreatedAt { get; init; }
    
    public DateTimeOffset UpdatedAt { get; init; }
        
    public DateTimeOffset? ActualStartDate { get; init; }
    
    public DateTimeOffset? ActualEndDate { get; init; }
    
    public WorkStatus WorkStatus { get; init; }
}