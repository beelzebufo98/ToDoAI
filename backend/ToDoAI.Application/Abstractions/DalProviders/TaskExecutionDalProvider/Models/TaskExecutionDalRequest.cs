namespace ToDoAI.Application.Abstractions.DalProviders.TaskExecutionDalProvider.Models;

public sealed record TaskExecutionDalRequest
{
    public Guid TaskId { get; init; }
    
    public Guid? ScheduleId { get; init; }
    
    public int ActualMinutes { get; init; }
    
    public int EnergyAfter { get; init; }
    
    public int StressAfter { get; init; }

    public DateTimeOffset CreatedAt { get; init; }
}