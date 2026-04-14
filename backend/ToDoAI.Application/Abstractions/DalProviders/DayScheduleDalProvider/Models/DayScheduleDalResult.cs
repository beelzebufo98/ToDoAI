namespace ToDoAI.Application.Abstractions.DalProviders.DayScheduleDalProvider.Models;

public sealed record DayScheduleDalResult
{
    public Guid DayScheduleId { get; init; }
    
    public Guid UserId { get; init; }
    
    public DateOnly ScheduleDate { get; init; }
    
    public int Version { get; init; }
    
    public bool IsActiveVersion { get; init; }
    
    public DateTimeOffset CreateDate { get; init; }

    public IList<ScheduleDalResult> Blocks { get; init; } = [];
}