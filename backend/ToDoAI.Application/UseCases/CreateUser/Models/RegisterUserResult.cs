using ToDoAI.ToDoAI.Domain.Enums;

namespace ToDoAI.ToDoAI.Application.UseCases.CreateUser.Models;

public sealed record RegisterUserResult
{
    public bool Success { get; init; }
    
    public ErrorCodes? Error { get; init; }
}