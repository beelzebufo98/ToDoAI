using ToDoAI.Domain.Enums;

namespace ToDoAI.Application.UseCases.AssistTask.Models;

public sealed record AssistTaskBlResponse
{
    public string SuggestedTitle { get; set; } = string.Empty;

    public string SuggestedDescription { get; set; } = string.Empty;

    public int SuggestedEstimatedMinutes { get; set; }

    public int SuggestedComplexityLevel { get; set; }

    public int SuggestedPriority { get; set; }
    
    public string Reasoning { get; set; } = string.Empty;
    
    public ErrorCodes? Error { get; set; }
}