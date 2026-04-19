namespace ToDoAI.Application.UseCases.GenerateSchedule.Models;

public sealed record DayScheduleBlResult
{
    public Guid DayScheduleId { get; init; }
    
    public Guid UserId { get; init; }
    
    public DateOnly ScheduleDate { get; init; }
    
    public int Version { get; init; }
    
    public bool IsActiveVersion { get; init; }
    
    public DateTimeOffset CreateDate { get; init; }

    public IList<ScheduleBlResult> Blocks { get; init; } = [];
}