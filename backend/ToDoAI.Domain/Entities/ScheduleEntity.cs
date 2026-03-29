namespace ToDoAI.ToDoAI.Domain.Entities;

public sealed class ScheduleEntity
{
    public Guid Id { get; set; }

    public Guid DayScheduleId { get; set; }

    public DayScheduleEntity DaySchedule { get; set; } = default!;

    public Guid TaskId { get; set; }

    public TaskEntity Task { get; set; }
    
    public DateTimeOffset? Start { get; set; }
    
    public DateTimeOffset? End { get; set; }
}
