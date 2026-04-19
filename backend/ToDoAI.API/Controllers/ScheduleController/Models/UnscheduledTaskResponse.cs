namespace ToDoAI.API.Controllers.ScheduleController.Models;

public sealed class UnscheduledTaskResponse
{
    public Guid TaskId { get; init; }

    public string TaskTitle { get; init; } = default!;

    public string Description { get; init; } = default!;

    public int EstimatedMinutes { get; init; }
}