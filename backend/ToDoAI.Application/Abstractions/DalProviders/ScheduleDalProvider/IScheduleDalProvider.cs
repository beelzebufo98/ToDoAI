using ToDoAI.Application.Abstractions.DalProviders.ScheduleDalProvider.Models;

namespace ToDoAI.Application.Abstractions.DalProviders.ScheduleDalProvider;

public interface IScheduleDalProvider
{
    Task<ScheduleDal?> GetSchedule(Guid taskId, CancellationToken cancellationToken);
}