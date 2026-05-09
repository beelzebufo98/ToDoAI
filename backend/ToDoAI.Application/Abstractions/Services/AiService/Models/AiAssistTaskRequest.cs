namespace ToDoAI.Application.Abstractions.Services.AiService.Models;

public sealed record AiAssistTaskRequest
{
    public required string Title { get; set; }
    
    public required string Description { get; set; }
    
    public required DateTimeOffset DeadlineAt { get; set; }
}