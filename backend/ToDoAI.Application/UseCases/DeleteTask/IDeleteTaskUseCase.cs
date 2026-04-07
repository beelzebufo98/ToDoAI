using ToDoAI.Application.UseCases.DeleteTask.Models;

namespace ToDoAI.Application.UseCases.DeleteTask;

public interface IDeleteTaskUseCase
{
    Task<DeleteTaskResult> DeleteTask(Guid taskId, Guid userId, CancellationToken cancellationToken);
}