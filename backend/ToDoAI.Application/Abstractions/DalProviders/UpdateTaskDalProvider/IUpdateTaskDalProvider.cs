using ToDoAI.Application.Abstractions.DalProviders.GetTaskDalProvider.Models;
using ToDoAI.Application.Abstractions.DalProviders.UpdateTaskDalProvider.Models;

namespace ToDoAI.Application.Abstractions.DalProviders.UpdateTaskDalProvider;

public interface IUpdateTaskDalProvider
{
    Task<TaskDal?> UpdateTask(UpdateTaskDalRequest updateTask, CancellationToken cancellation);
}