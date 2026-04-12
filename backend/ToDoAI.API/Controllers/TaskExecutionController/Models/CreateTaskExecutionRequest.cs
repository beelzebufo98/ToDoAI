namespace ToDoAI.API.Controllers.TaskExecutionController.Models;

public sealed class CreateTaskExecutionRequest
{
    public string? ScheduledId { get; set; }
    
    public int ActualMinutes { get; set; }
    
    public int StressAfter {get; set;}
    
    public int EnergyAfter {get; set;}
}