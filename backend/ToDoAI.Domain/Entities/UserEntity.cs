namespace ToDoAI.ToDoAI.Domain.Entities;

public sealed class UserEntity
{
    public Guid Id { get; set; }
    
    public string UserName { get; set; } = default!;
    
    public string FirstName { get; set; } = default!;
    
    public string? LastName { get; set; }
    
    public string? Email { get; set; }
    
    public string PasswordHash { get; set; } = default!;
    
    public ICollection<TaskEntity> Tasks { get; set; } = [];
    
    public ICollection<UserStateEntity> States { get; set; } = [];
}
