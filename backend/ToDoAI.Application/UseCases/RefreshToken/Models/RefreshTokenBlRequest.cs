namespace ToDoAI.Application.UseCases.RefreshToken.Models;

public sealed record RefreshTokenBlRequest
{
    public required Guid UserId { get; init; }
    
    public required string RefreshTokenHash { get; init; }
    
    public DateTimeOffset ExpiresAt { get; init; }
    
    public DateTimeOffset CreatedAt { get; init; }
}