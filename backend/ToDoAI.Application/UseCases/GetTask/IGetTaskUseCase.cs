using ToDoAI.ToDoAI.Application.UseCases.GetTask.Models;

namespace ToDoAI.ToDoAI.Application.UseCases.GetTask;

public interface IGetTaskUseCase
{
    Task<GetTaskResult> GetTask(Guid taskId, Guid userId, CancellationToken cancellationToken);

    Task<GetTasksResult> GetTasks(Guid userId, CancellationToken cancellationToken);
}
