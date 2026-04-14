using ToDoAI.Application.Abstractions.DalProviders.DayScheduleDalProvider.Models;
using ToDoAI.Domain.Enums;

namespace ToDoAI.Application.UseCases.GenerateSchedule.Models;

public sealed record GenerateScheduleBlResult
{
    public DayScheduleDalResult DaySchedule { get; init; } = null!;

    public ErrorCodes? ErrorCode { get; init; }
}