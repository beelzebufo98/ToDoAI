namespace ToDoAI.ToDoAI.Application.UseCases.CreateUser.Models;

public sealed record UserHash
{
    public required Guid Id { get; init; }
    
    public required string UserName { get; init; }
    
    public required string FirstName { get; init; }
}