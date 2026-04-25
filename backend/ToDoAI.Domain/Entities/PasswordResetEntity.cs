namespace ToDoAI.Domain.Entities;

public sealed class PasswordResetEntity
{
    public Guid Id { get; set; }
    
    public Guid UserId { get; set; }
    
    public string CodeHash { get; set; } = default!;

    public DateTimeOffset ExpiresAt { get; set; }
    
    public DateTimeOffset SentAt { get; set; }
    
    public int Attempts { get; set; }
}