namespace ToDoAI.API.Controllers.Auth.Models;

public sealed class ForgotPasswordRequest
{
    public required string Email { get; init; }
}
