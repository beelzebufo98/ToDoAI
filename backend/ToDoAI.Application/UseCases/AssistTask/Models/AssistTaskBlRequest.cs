namespace ToDoAI.Application.UseCases.AssistTask.Models;

public sealed record AssistTaskBlRequest
{
    public required Guid UserId { get; set; }

    public required string Title { get; set; }
    
    public required string Description { get; set; }
    
    public required DateTimeOffset DeadlineAt { get; set; }
}