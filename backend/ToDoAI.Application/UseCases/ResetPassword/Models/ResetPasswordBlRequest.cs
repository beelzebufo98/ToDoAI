namespace ToDoAI.Application.UseCases.ResetPassword.Models;

public sealed record ResetPasswordBlRequest
{
    public required string Email { get; init; }

    public required string Code { get; init; }

    public required string NewPassword { get; init; }
}
