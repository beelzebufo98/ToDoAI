using ToDoAI.Application.Abstractions.DalProviders.DayScheduleDalProvider.Models;

namespace ToDoAI.Application.Abstractions.DalProviders.DayScheduleDalProvider;

public interface IDayScheduleDalProvider
{
    Task<DayScheduleDalResult> CreateDaySchedule(ScheduleDalRequest request, CancellationToken cancellationToken);

    Task<DayScheduleDalResult?> GetDaySchedule(Guid userId, DateOnly scheduleDate,
        CancellationToken cancellationToken);
}
