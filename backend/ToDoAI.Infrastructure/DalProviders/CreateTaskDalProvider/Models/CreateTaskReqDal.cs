using TaskStatus = ToDoAI.ToDoAI.Domain.Enums.TaskStatus;

namespace ToDoAI.ToDoAI.Infrastructure.DalProviders.CreateTaskDalProvider.Models;

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
    
    public TaskStatus Status { get; init; } =  TaskStatus.New;
    
    public Guid UserId { get; init; }
}