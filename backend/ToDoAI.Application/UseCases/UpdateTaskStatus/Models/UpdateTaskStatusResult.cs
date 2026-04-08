using ToDoAI.Application.UseCases.GetTask.Models;
using ToDoAI.Domain.Enums;

namespace ToDoAI.Application.UseCases.UpdateTaskStatus.Models;

public sealed record UpdateTaskStatusResult
{
    public TaskWorkStatusBlFilters WorkStatus { get; init; }
    
    public ErrorCodes? ErrorCode { get; init; }
}