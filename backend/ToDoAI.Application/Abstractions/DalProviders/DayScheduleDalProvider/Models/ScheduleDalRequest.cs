using ToDoAI.Application.Abstractions.DalProviders.GetTaskDalProvider.Models;

namespace ToDoAI.Application.Abstractions.DalProviders.DayScheduleDalProvider.Models;

public sealed record ScheduleDalRequest
{
    public IReadOnlyCollection<TaskDal> TaskList { get; init; } = [];
    
    public Guid UserId { get; init; }
    
    public DateOnly ScheduleDate { get; init; }

    public DateTimeOffset StartAt { get; init; }
}
