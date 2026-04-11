using ToDoAI.Domain.Enums;

namespace ToDoAI.Application.UseCases.UserStateUseCase.Models;

public sealed record UserStateBlResult
{
    public UserStateResult UserState { get; init; } = null!;

    public ErrorCodes? ErrorCode { get; init; }
}