namespace ToDoAI.ToDoAI.Domain.Entities;

public sealed class UserEntity
{
    public Guid UserId { get; set; }
    
    public string FirstName { get; set; } = default!;
    
    public string LastName { get; set; } = default!;
    
    public string Email { get; set; } = default!;
    
    public string PasswordHash { get; set; } = default!;
}