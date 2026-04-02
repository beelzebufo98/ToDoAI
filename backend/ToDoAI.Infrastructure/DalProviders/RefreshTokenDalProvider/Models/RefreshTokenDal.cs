namespace ToDoAI.ToDoAI.Infrastructure.DalProviders.RefreshTokenDalProvider.Models;

public sealed record RefreshTokenDal
{
    public required Guid UserId { get; init; }
    
    public required string RefreshTokenHash { get; init; }
    
    public required DateTimeOffset ExpiresAt { get; init; }
    
    public required DateTimeOffset CreatedAt { get; init; }
}
