using ToDoAI.Domain.Enums;

namespace ToDoAI.Application.Abstractions.DalProviders.TaskWorkSessionDalProvider.Models;

public sealed record CancelTaskWorkSessionDalResult
{
    public TaskWorkSessionDal? Session { get; init; }

    public ErrorCodes? ErrorCode { get; init; }
}