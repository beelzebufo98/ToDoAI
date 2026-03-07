namespace ToDoAI.ToDoAI.API.Controllers.Auth.Models;

public sealed class RegisterUserRequest
{
    public required string UserName { get; set; } = default!;
    public required string FirstName { get; set; } = default!;
    public required string? LastName { get; set; }
    
    public required string Password { get; set; } = default!;
}