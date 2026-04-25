using ToDoAI.Application.Abstractions.DalProviders.TaskExecutionDalProvider.Models;

namespace ToDoAI.Application.Abstractions.DalProviders.TaskExecutionDalProvider;

public interface ITaskExecutionDalProvider
{
    Task<TaskExecutionDal?> CreateTaskExecution(TaskExecutionDalRequest request, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<RecentTaskExecutionDal>> GetRecentTaskExecutions(
        Guid userId,
        int count,
        CancellationToken cancellationToken);
}
