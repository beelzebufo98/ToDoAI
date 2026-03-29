namespace ToDoAI.ToDoAI.API.Controllers.Auth.Models;

public sealed class LoginUserRequest
{
    public required string Username { get; set; }
    
    public required string Password { get; set; }
}