using ToDoAI.ToDoAI.Domain;

namespace ToDoAI.ToDoAI.Application.UseCases.CreateUser.Models;

public sealed record RegisterUserResult
{
    public bool Success { get; init; }
    
    public ErrorCodes? Error { get; init; }
}