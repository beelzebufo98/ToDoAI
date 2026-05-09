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