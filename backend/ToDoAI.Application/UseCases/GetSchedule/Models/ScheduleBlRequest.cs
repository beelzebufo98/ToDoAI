namespace ToDoAI.Application.UseCases.GetSchedule.Models;

public sealed record ScheduleBlRequest
{
    public DateOnly ScheduleDate { get; init; }
    
    public Guid UserId { get; init; }
}