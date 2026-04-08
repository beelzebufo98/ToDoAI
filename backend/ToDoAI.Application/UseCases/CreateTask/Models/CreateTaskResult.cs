using ErrorCodes = ToDoAI.Domain.Enums.ErrorCodes;

namespace ToDoAI.Application.UseCases.CreateTask.Models;

public sealed record CreateTaskResult
{
    public Guid? TaskId { get; init; }
    
    public ErrorCodes? ErrorCode { get; init; }
}