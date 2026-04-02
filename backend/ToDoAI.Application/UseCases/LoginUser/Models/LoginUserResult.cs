using ToDoAI.ToDoAI.Domain.Enums;

namespace ToDoAI.ToDoAI.Application.UseCases.LoginUser.Models;

public sealed record LoginUserResult
{
    public string? AccessToken { get; init; }
    
    public string? RefreshToken { get; init; }
    public bool Success { get; init; }
    
    public ErrorCodes? Error { get; init; }
}