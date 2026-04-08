using ToDoAI.Domain.Enums;

namespace ToDoAI.Application.Abstractions.DalProviders.CreateTaskDalProvider.Models;

public sealed record CreateTaskReqDal
{
    public Guid TaskId { get; init; }
    
    public required string  Title { get; init; }
    
    public required string Description { get; init; }
    
    public int EstimatedMinutes { get; init; }
    
    public int ComplexityLevel { get; init; }
    
    public int PriorityLevel { get; init; }
    
    public DateTimeOffset CreatedAt { get; init; }
    
    public DateTimeOffset UpdatedAt { get; init; }
    
    public WorkStatus Status { get; init; } = WorkStatus.New;
    
    public Guid UserId { get; init; }
}
