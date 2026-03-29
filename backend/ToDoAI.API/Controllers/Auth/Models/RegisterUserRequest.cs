namespace ToDoAI.ToDoAI.API.Controllers.Auth.Models;

public sealed class RegisterUserRequest
{
    public required string UserName { get; init; }
    
    public required string FirstName { get; init; }
    
    public string? LastName { get; init; }
    
    public required string Password { get; init; }
}