using ToDoAI.Domain.Enums;

namespace ToDoAI.Domain.Entities;

public sealed class TaskEntity
{
    public Guid Id { get; init; }

    public string Title { get; set; } = default!;

    public string Description { get; set; } = default!;

    public int EstimatedMinutes { get; set; }

    public int ComplexityLevel { get; set; }

    public int Priority { get; set; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset? ActualStartDate { get; set; }

    public DateTimeOffset? ActualEndDate { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public DateTimeOffset DeadlineAt { get; set; }

    public WorkStatus WorkStatus { get; set; }
    
    public DateTimeOffset? DeletedAt { get; set; }
    
    public Guid UserId { get; init; }

    public UserEntity User { get; init; } = default!;
}
