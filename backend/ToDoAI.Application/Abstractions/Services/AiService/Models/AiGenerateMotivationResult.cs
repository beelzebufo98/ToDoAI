namespace ToDoAI.Application.Abstractions.Services.AiService.Models;

public sealed record AiGenerateMotivationResult
{
    public bool UsedAi { get; init; }

    public string? FallbackReason { get; init; }

    public AiGenerateMotivationResponse? Response { get; init; }
}