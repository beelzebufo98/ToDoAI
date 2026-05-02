using ToDoAI.Domain.Enums;

namespace ToDoAI.Application.UseCases.UserUseCase.Models;

public sealed record UserBlResult
{
    public UserResult UserResult { get; init; }
    
    public ErrorCodes? Error { get; init; }
}