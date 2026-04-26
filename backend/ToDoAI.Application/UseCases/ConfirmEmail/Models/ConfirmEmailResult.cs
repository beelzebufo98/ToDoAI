using ToDoAI.Domain.Enums;

namespace ToDoAI.Application.UseCases.ConfirmEmail.Models;

public sealed record ConfirmEmailResult
{
    public bool Success { get; init; }

    public ErrorCodes? Error { get; init; }
}
