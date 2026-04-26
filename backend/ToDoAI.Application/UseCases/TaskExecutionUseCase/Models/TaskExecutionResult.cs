namespace ToDoAI.Application.UseCases.TaskExecutionUseCase.Models;

public sealed record TaskExecutionResult
{
    public Guid TaskExecutionId { get; init; }

    public Guid TaskId { get; init; }
    
    public Guid? ScheduleId { get; init; }
    
    public int ActualMinutes { get; init; }
    
    public int EnergyAfter { get; init; }
    
    public int StressAfter { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public string MotivationMessage { get; init; } = string.Empty;
}