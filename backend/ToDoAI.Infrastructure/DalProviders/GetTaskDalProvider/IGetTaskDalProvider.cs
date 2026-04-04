using ToDoAI.ToDoAI.Application.UseCases.GetTask.Models;
using ToDoAI.ToDoAI.Infrastructure.DalProviders.GetTaskDalProvider.Models;

namespace ToDoAI.ToDoAI.Infrastructure.DalProviders.GetTaskDalProvider;

public interface IGetTaskDalProvider
{
    Task<TaskDal?> GetTask(Guid taskId, Guid userId, CancellationToken cancellationToken);
    
    Task<IReadOnlyCollection<TaskDal>> GetTasks(Guid userId, TaskFiltersBlRequest filters, CancellationToken cancellationToken);
}
