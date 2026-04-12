namespace ToDoAI.Application.Abstractions.DalProviders.ScheduleDalProvider.Models;

public sealed record ScheduleDal
{
    public Guid Id { get; set; }

    public Guid DayScheduleId { get; set; }

    public Guid TaskId { get; set; }
    
    public DateTimeOffset Start { get; set; }
    
    public DateTimeOffset End { get; set; }
}