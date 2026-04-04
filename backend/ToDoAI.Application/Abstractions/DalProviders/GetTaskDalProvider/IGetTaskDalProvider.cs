using ToDoAI.Application.Abstractions.DalProviders.GetTaskDalProvider.Models;
using ToDoAI.Application.UseCases.GetTask.Models;

namespace ToDoAI.Application.Abstractions.DalProviders.GetTaskDalProvider;

public interface IGetTaskDalProvider
{
    Task<TaskDal?> GetTask(Guid taskId, Guid userId, CancellationToken cancellationToken);
    
    Task<IReadOnlyCollection<TaskDal>> GetTasks(Guid userId, TaskFiltersBlRequest filters, CancellationToken cancellationToken);
}
