using ToDoAI.Application.UseCases.GenerateSchedule.Models;
using ToDoAI.Domain.Enums;

namespace ToDoAI.Application.UseCases.GetSchedule.Models;

public sealed record GetScheduleBlResult
{
    public DayScheduleBlResult DaySchedule { get; init; } = null!;

    public ErrorCodes? ErrorCode { get; init; }
}