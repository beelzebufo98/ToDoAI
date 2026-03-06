namespace ToDoAI.ToDoAI.Application.Services.AccountService.Models;

public sealed record RegisterUserBlRequest
{
    public required string UserName { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
}