namespace ToDoAI.API.Controllers.Auth.Models;

public sealed class ResendConfirmationCodeRequest
{
    public required string Email { get; init; }
}