using ToDoAI.Application.UseCases.GetTask.Models;

namespace ToDoAI.Application.UseCases.UpdateTaskStatus.Models;

public sealed record UpdateTaskStatusBlRequest
{
    public Guid TaskId { get; init; }
    
    public Guid UserId { get; init; }
    
    public TaskWorkStatusBlFilters TaskWorkStatus { get; init; }
}