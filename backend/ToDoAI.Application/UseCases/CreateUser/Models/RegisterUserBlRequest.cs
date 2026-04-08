namespace ToDoAI.Application.UseCases.CreateUser.Models;

public sealed record RegisterUserBlRequest
{
    public required string UserName { get; init; }
    public required string FirstName { get; init; }
    public string? LastName { get; init; }
    
    public required string Password { get; init; }
}