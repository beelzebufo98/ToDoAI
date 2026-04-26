namespace ToDoAI.Application.Abstractions.Services.AiService.Models;

public sealed record AiGenerateMotivationRequest
{
    public required string Trigger { get; init; }
}