namespace ToDoAI.Application.Services.AiService.Settings;

public sealed class AiServiceSettings
{
    public bool Enabled { get; init; }

    public string BaseUrl { get; init; } = string.Empty;

    public string GenerateSchedulePath { get; init; } = "/api/v1/ai/schedule/generate";

    public int TimeoutSeconds { get; init; } = 15;
}
