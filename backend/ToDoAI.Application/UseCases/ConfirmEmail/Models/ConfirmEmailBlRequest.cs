namespace ToDoAI.Application.UseCases.ConfirmEmail.Models;

public sealed record ConfirmEmailBlRequest
{
    public required string Email { get; init; }

    public required string Code { get; init; }
}
