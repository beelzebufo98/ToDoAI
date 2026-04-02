namespace ToDoAI.ToDoAI.Application.UseCases.RefreshTokenUseCase.Models;

public sealed record RefreshTokenBlResult
{
    public required Guid UserId { get; init; }
    
    public required string RefreshTokenHash { get; init; }
    
    public required DateTimeOffset ExpiresAt { get; init; }
    
    public required DateTimeOffset CreatedAt { get; init; }
}