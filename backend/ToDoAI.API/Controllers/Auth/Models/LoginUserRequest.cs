namespace ToDoAI.API.Controllers.Auth.Models;

public sealed class LoginUserRequest
{
    public required string UserName { get; set; }
    
    public required string Password { get; set; }
}