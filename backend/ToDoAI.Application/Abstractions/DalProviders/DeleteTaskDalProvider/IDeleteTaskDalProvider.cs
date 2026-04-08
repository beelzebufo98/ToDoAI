using ToDoAI.Application.Abstractions.DalProviders.DeleteTaskDalProvider.Models;

namespace ToDoAI.Application.Abstractions.DalProviders.DeleteTaskDalProvider;

public interface IDeleteTaskDalProvider
{
    Task<DeleteTaskDal?> DeleteTask(Guid userId, Guid taskId, CancellationToken cancellationToken);
}