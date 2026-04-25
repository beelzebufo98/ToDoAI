namespace ToDoAI.Application.Abstractions.Services.AiService.Models;

public sealed record AiGenerateScheduleResponse
{
    public IReadOnlyCollection<AiScheduledBlockResponse> Scheduled { get; init; } = [];

    public IReadOnlyCollection<AiUnscheduledTaskResponse> Unscheduled { get; init; } = [];

    public AiScheduleSummaryResponse? Summary { get; init; }
}

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

public sealed record AiUnscheduledTaskResponse
{
    public Guid TaskId { get; init; }

    public string Title { get; init; } = default!;

    public int RemainingMinutes { get; init; }

    public string Reason { get; init; } = default!;

    public string? Reasoning { get; init; }
}

public sealed record AiScheduleSummaryResponse
{
    public DateOnly ScheduleDate { get; init; }

    public int AvailableMinutes { get; init; }

    public int PlannedMinutes { get; init; }

    public int ScheduledCount { get; init; }

    public int UnscheduledCount { get; init; }

    public IReadOnlyCollection<string> Explanations { get; init; } = [];

    public string? PlannerModel { get; init; }

    public bool UsedFallbackRanking { get; init; }

    public DateTimeOffset GeneratedAt { get; init; }
}
