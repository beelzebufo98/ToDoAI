namespace ToDoAI.ToDoAI.Domain.Entities;

public sealed class RefreshSessionEntity
{
    public Guid Id { get; init; }
    
    public Guid UserId { get; init; }

    public string TokenHash { get; init; } = default!;
    
    public DateTimeOffset ExpiresAt { get; init; }
    
    public DateTimeOffset CreatedAt { get; init; }
}