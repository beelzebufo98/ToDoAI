using ToDoAI.Domain.Enums;

namespace ToDoAI.Application.UseCases.GenerateSchedule.Models;

public sealed record GenerateScheduleBlResult
{
    public DayScheduleBlResult DaySchedule { get; init; } = null!;

    public IReadOnlyCollection<UnscheduledTaskBlResult> Unscheduled { get; init; } = [];

    public ErrorCodes? ErrorCode { get; init; }
}