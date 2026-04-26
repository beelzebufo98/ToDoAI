namespace ToDoAI.Application.Abstractions.DalProviders.PasswordResetDalProvider.Models;

public sealed record PasswordResetDal
{
    public required Guid Id { get; init; }

    public required Guid UserId { get; init; }

    public required string CodeHash { get; init; }

    public required DateTimeOffset ExpiresAt { get; init; }

    public required DateTimeOffset SentAt { get; init; }

    public required int Attempts { get; init; }
}
