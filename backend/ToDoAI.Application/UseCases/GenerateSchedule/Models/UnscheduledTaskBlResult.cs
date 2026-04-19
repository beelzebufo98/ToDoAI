namespace ToDoAI.Application.UseCases.GenerateSchedule.Models;

public sealed record UnscheduledTaskBlResult
{
    public Guid TaskId { get; init; }

    public string TaskTitle { get; init; } = default!;

    public string Description { get; init; } = default!;

    public int EstimatedMinutes { get; init; }
}