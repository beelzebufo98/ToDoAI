using ToDoAI.Application.Abstractions.DalProviders.UpdateTaskStatusDalProvider.Models;

namespace ToDoAI.Application.Abstractions.DalProviders.UpdateTaskStatusDalProvider;

public interface IUpdateTaskStatusDalProvider
{
    Task<UpdateTaskStatusDal?> UpdateTaskStatus(UpdateTaskStatusDalRequest updateTask,
        CancellationToken cancellationToken);
}