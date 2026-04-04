using ToDoAI.Application.Abstractions.DalProviders.CreateTaskDalProvider.Models;

namespace ToDoAI.Application.Abstractions.DalProviders.CreateTaskDalProvider;

public interface ICreateTaskDalProvider
{
    Task<CreateTaskDal> CreateTask(CreateTaskReqDal task, CancellationToken cancellationToken);
}
