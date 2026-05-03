using ToDoAI.Domain.Enums;

namespace ToDoAI.Application.UseCases.UserStateUseCase.Models;

public sealed record UserStateStatisticsBlResult
{
    public UserStateStatisticsResult? UserStateStatistics { get; init; }
    
    public ErrorCodes?  ErrorCode { get; init; }
}