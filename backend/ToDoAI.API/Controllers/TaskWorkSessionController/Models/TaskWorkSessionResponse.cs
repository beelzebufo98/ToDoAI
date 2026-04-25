using ToDoAI.Domain.Enums;

namespace ToDoAI.API.Controllers.TaskWorkSessionController.Models;

public sealed class TaskWorkSessionResponse
{
    public Guid SessionId { get; init; }
    
    public Guid UserId { get; init; }
    
    public Guid TaskId { get; init; }

    public Guid? ScheduleBlockId { get; init; }
    
    public DateTimeOffset StartedAt { get; init; }
    
    public DateTimeOffset? EndedAt { get; init; }
    
    public int? SpentMinutes { get; init; }
    
    public TaskWorkSessionStatus Status { get; init; }
    
    public TaskWorkSessionSource Source { get; init; }
    
    public DateTimeOffset CreatedAt { get; init; }
    
    public string? Title { get; init; }
    public int? TaskEstimatedMinutes { get; init; }
    
    public int? TaskActualSpentMinutes { get; init; }
}
