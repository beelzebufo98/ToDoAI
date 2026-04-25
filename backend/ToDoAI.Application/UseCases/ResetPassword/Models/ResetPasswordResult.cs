using ToDoAI.Domain.Enums;

namespace ToDoAI.Application.UseCases.ResetPassword.Models;

public sealed record ResetPasswordResult
{
    public bool Success { get; init; }

    public ErrorCodes? Error { get; init; }
}
