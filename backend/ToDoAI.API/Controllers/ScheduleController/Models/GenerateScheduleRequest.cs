namespace ToDoAI.API.Controllers.ScheduleController.Models;

public sealed class GenerateScheduleRequest
{
    public required DateOnly ScheduleDate { get; init; }

    public required DateTimeOffset StartAt { get; init; }

    public IReadOnlyCollection<Guid> TaskIds { get; init; } = [];

}
