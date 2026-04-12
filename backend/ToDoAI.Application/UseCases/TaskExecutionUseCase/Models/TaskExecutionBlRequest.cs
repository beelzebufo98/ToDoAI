namespace ToDoAI.Application.UseCases.TaskExecutionUseCase.Models;

public sealed record TaskExecutionBlRequest
{
    public Guid UserId { get; init; }
    
    public Guid TaskId { get; init; }
    
    public Guid? ScheduleId { get; init; }
    
    public int ActualMinutes { get; set; }
    
    public int StressAfter {get; set;}
    
    public int EnergyAfter {get; set;}
}