using ToDoAI.Domain.Enums;

namespace ToDoAI.Application.UseCases.UserStateUseCase.Models;

public sealed record UserStateHistoryBlResult
{
    public IReadOnlyCollection<UserStateResult> UserState { get; init; } = [];

    public ErrorCodes? ErrorCode { get; init; }
}