using ToDoAI.Domain.Enums;

namespace ToDoAI.Application.Abstractions.Services.AiService.Models;

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