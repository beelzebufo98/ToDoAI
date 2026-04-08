namespace ToDoAI.Application.Abstractions.DalProviders.RefreshTokenDalProvider.Models;

public sealed record RefreshTokenRequestDal
{
    public required Guid UserId { get; init; }
    
    public required string RefreshTokenHash { get; init; }
    
    public DateTimeOffset ExpiresAt { get; init; }
    
    public DateTimeOffset CreatedAt { get; init; }
}
