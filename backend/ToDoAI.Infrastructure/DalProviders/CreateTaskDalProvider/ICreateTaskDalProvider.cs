using ToDoAI.ToDoAI.Infrastructure.DalProviders.CreateTaskDalProvider.Models;

namespace ToDoAI.ToDoAI.Infrastructure.DalProviders.CreateTaskDalProvider;

public interface ICreateTaskDalProvider
{
    Task<CreateTaskDal> CreateTask(CreateTaskReqDal task, CancellationToken cancellationToken);
}