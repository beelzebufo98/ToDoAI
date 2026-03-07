namespace ToDoAI.ToDoAI.Domain.Entities;

public sealed class DayScheduleEntity
{
    public Guid Id { get; set; }

    public DateOnly Date { get; set; }

    public int Version { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public Guid UserId { get; set; }

    public UserEntity User { get; set; } = default!;

    public ICollection<ScheduleEntity> Blocks { get; set; } = [];
}
