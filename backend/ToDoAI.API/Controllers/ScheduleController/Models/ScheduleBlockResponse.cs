namespace ToDoAI.API.Controllers.ScheduleController.Models;

public sealed class ScheduleBlockResponse
{
    public Guid ScheduleId { get; init; }

    public Guid TaskId { get; init; }

    public DateTimeOffset StartAt { get; init; }

    public DateTimeOffset EndAt { get; init; }
    
    public string? TaskTitle { get; init; }
    
    public string? TaskDescription { get; init; }
}
