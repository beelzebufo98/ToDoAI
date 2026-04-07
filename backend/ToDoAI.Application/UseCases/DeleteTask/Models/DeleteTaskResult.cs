using ToDoAI.Domain.Enums;

namespace ToDoAI.Application.UseCases.DeleteTask.Models;

public sealed record DeleteTaskResult
{
    public Guid? TaskId { get; init; }
    public ErrorCodes? ErrorCode { get; init; }
}