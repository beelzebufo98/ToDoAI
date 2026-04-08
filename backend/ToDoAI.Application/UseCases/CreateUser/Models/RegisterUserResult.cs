using ToDoAI.Domain.Enums;

namespace ToDoAI.Application.UseCases.CreateUser.Models;

public sealed record RegisterUserResult
{
    public bool Success { get; init; }
    
    public ErrorCodes? Error { get; init; }
}