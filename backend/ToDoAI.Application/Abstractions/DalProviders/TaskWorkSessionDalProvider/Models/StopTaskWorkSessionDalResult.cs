using ToDoAI.Domain.Enums;

namespace ToDoAI.Application.Abstractions.DalProviders.TaskWorkSessionDalProvider.Models;

public sealed record StopTaskWorkSessionDalResult
{
    public TaskWorkSessionDal? Session { get; init; }

    public ErrorCodes? ErrorCode { get; init; }
}
