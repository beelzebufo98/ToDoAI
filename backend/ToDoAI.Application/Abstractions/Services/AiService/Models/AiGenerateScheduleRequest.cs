using ToDoAI.Domain.Enums;

namespace ToDoAI.Application.Abstractions.Services.AiService.Models;

public sealed record AiGenerateScheduleRequest
{
    public required DateOnly ScheduleDate { get; init; }

    public required DateTimeOffset DayStartAt { get; init; }

    public DateTimeOffset? DayEndAt { get; init; }

    public AiUserStateRequest? UserState { get; init; }

    public IReadOnlyCollection<AiRecentTaskExecutionRequest> RecentExecutions { get; init; } = [];

    public IReadOnlyCollection<AiPlanningTaskRequest> Tasks { get; init; } = [];
}

public sealed record AiPlanningTaskRequest
{
    public required Guid Id { get; init; }

    public required string Title { get; init; }

    public required string Description { get; init; }

    public int EstimatedMinutes { get; init; }

    public int RemainingMinutes { get; init; }

    public int Priority { get; init; }

    public int ComplexityLevel { get; init; }

    public DateTimeOffset? DeadlineAt { get; init; }

    public WorkStatus WorkStatus { get; init; }
}

public sealed record AiUserStateRequest
{
    public int SleepMinutes { get; init; }

    public int EnergyLevel { get; init; }

    public int StressLevel { get; init; }

    public int MotivationLevel { get; init; }

    public int ConcentrationLevel { get; init; }
}

public sealed record AiRecentTaskExecutionRequest
{
    public required Guid TaskId { get; init; }

    public required string TaskTitle { get; init; }

    public int EstimatedMinutes { get; init; }

    public int ActualMinutes { get; init; }

    public int Priority { get; init; }

    public int ComplexityLevel { get; init; }

    public int EnergyAfter { get; init; }

    public int StressAfter { get; init; }

    public DateTimeOffset CreatedAt { get; init; }
}
