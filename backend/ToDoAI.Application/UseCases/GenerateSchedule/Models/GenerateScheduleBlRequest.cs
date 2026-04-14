namespace ToDoAI.Application.UseCases.GenerateSchedule.Models;

public sealed record GenerateScheduleBlRequest
{
    public Guid UserId { get; init; }
    
    public required DateOnly ScheduleDate { get; init; }

    public required DateTimeOffset StartAt { get; init; }

    public IReadOnlyCollection<Guid> TaskIds { get; init; } = [];
}
