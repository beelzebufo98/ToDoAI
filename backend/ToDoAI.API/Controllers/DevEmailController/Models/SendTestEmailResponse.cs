namespace ToDoAI.API.Controllers.DevEmailController.Models;

public sealed class SendTestEmailResponse
{
    public string Email { get; init; } = string.Empty;

    public string Type { get; init; } = string.Empty;

    public string Code { get; init; } = string.Empty;
}
