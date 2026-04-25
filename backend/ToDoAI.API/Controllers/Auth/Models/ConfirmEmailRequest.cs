namespace ToDoAI.API.Controllers.Auth.Models;

public sealed class ConfirmEmailRequest
{
    public required string Email { get; init; }

    public required string Code { get; init; }
}
