using ToDoAI.Domain.Enums;

namespace ToDoAI.Application.UseCases.GetTask.Models;

public sealed record GetTaskResult
{
    public TaskResult TaskResult { get; init; } = null!;
    
    public ErrorCodes? ErrorCode { get; init; }
}