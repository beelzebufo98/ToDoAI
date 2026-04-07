namespace ToDoAI.Application.Abstractions.DalProviders.UpdateTaskStatusDalProvider.Models;

public sealed record UpdateTaskStatusDalRequest
{
    public Guid TaskId { get; init; }
    
    public Guid UserId { get; init; }
    
    public TaskWorkStatusDal WorkStatus { get; init; }
}