using ToDoAI.Domain.Enums;

namespace ToDoAI.Application.Abstractions.DalProviders.UpdateTaskStatusDalProvider.Models;

public sealed record UpdateTaskStatusDal
{
    public TaskWorkStatusDal  WorkStatus { get; init; }

    public ErrorCodes? ErrorCode { get; init; }
}