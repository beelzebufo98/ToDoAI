namespace ToDoAI.Application.Abstractions.DalProviders.TaskWorkSessionDalProvider.Models;

public sealed record GetTaskWorkSessionsDalResult
{
    public IReadOnlyList<TaskWorkSessionDal> TaskWorkSessions { get; init; } = [];
    
    public int? TotalCompletedMinutes { get; init; }
    
    public int? TotalCompletedSessions { get; init; }
    
    public int? TotalSessions { get; init; }
}