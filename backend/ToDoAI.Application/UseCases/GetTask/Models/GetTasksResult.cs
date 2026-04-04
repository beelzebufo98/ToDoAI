using ToDoAI.Domain.Enums;

namespace ToDoAI.Application.UseCases.GetTask.Models;

public sealed record GetTasksResult
{
    public IReadOnlyCollection<TaskResult> TaskResults { get; init; } = [];
    
    public ErrorCodes? ErrorCode { get; init; }
}