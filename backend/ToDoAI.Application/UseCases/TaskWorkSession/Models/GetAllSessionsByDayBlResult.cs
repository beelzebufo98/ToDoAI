using ToDoAI.Domain.Enums;

namespace ToDoAI.Application.UseCases.TaskWorkSession.Models;

public sealed record GetAllSessionsByDayBlResult
{
    public IReadOnlyList<SessionBlResult> Sessions { get; init; } = [];
    
    public int? TotalCompletedMinutes { get; init; }
    
    public int? TotalCompletedSessions { get; init; }
    
    public int? TotalSessions { get; init; }
    
    public ErrorCodes?  ErrorCode { get; init; }
}