namespace ToDoAI.Application.Abstractions.Services.AiService.Models;

public sealed record AiScheduledBlockResponse
{
    public Guid TaskId { get; init; }

    public string Title { get; init; } = default!;

    public DateTimeOffset StartAt { get; init; }

    public DateTimeOffset EndAt { get; init; }

    public int PlannedMinutes { get; init; }

    public int Priority { get; init; }

    public string? Reasoning { get; init; }
}