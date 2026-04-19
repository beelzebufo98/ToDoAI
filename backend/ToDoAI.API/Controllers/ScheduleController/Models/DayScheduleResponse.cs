namespace ToDoAI.API.Controllers.ScheduleController.Models;

public sealed class DayScheduleResponse
{
    public Guid DayScheduleId { get; init; }

    public DateOnly ScheduleDate { get; init; }

    public int Version { get; init; }

    public bool IsActiveVersion { get; init; }

    public DateTimeOffset CreateDate { get; init; }

    public IReadOnlyCollection<ScheduleBlockResponse> Blocks { get; init; } = [];
}