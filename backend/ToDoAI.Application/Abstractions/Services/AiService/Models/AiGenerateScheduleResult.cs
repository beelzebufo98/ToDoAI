namespace ToDoAI.Application.Abstractions.Services.AiService.Models;

public sealed record AiGenerateScheduleResult
{
    public bool UsedAi { get; init; }

    public string? FallbackReason { get; init; }

    public AiGenerateScheduleResponse? Response { get; init; }
}
