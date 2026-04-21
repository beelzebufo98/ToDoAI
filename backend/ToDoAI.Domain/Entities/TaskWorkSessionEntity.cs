using ToDoAI.Domain.Enums;

namespace ToDoAI.Domain.Entities;

public sealed class TaskWorkSessionEntity
{
    public Guid Id { get; init; }

    public Guid TaskId { get; init; }
    public TaskEntity Task { get; init; } = default!;

    public Guid UserId { get; init; }
    public UserEntity User { get; init; } = default!;

    public Guid? ScheduleBlockId { get; init; }
    
    public ScheduleEntity? ScheduleBlock { get; init; }

    public DateTimeOffset StartedAt { get; init; }
    public DateTimeOffset? EndedAt { get; set; }

    public int? SpentMinutes { get; set; }

    public TaskWorkSessionStatus Status { get; set; }
    public TaskWorkSessionSource Source { get; init; }

    public DateTimeOffset CreatedAt { get; init; }
}