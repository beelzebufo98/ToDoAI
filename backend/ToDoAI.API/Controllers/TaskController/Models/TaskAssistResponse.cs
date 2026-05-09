namespace ToDoAI.API.Controllers.TaskController.Models;

public sealed class TaskAssistResponse
{
    public required string SuggestedTitle { get; set; }

    public required string SuggestedDescription { get; set; }

    public required int SuggestedEstimatedMinutes { get; set; }

    public required int SuggestedComplexityLevel { get; set; }

    public required int SuggestedPriority { get; set; }
    
    public required string  Reasoning  { get; set; }
}