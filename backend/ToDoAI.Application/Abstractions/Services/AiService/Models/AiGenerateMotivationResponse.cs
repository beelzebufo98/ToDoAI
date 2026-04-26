namespace ToDoAI.Application.Abstractions.Services.AiService.Models;

public sealed record AiGenerateMotivationResponse
{
    public string Message { get; init; } = string.Empty;

    public string Model { get; init; } = string.Empty;
}