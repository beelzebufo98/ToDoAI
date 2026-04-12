namespace ToDoAI.API.Controllers.TaskExecutionController.Models;

public sealed class TaskExecutionResponse
{
    public Guid TaskExecutionId { get; init; }

    public Guid TaskId { get; init; }
    
    public Guid? ScheduleId { get; init; }
    
    public int ActualMinutes { get; init; }
    
    public int EnergyAfter { get; init; }
    
    public int StressAfter { get; init; }

    public DateTimeOffset CreatedAt { get; init; }
}