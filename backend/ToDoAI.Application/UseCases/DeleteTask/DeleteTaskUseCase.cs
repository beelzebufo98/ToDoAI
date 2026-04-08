using ToDoAI.Application.Abstractions.DalProviders.DeleteTaskDalProvider;
using ToDoAI.Application.UseCases.DeleteTask.Models;
using ToDoAI.Domain.Enums;

namespace ToDoAI.Application.UseCases.DeleteTask;

public sealed class DeleteTaskUseCase : IDeleteTaskUseCase
{
    private readonly IDeleteTaskDalProvider _deleteTaskDalProvider;

    public DeleteTaskUseCase(IDeleteTaskDalProvider deleteTaskDalProvider)
    {
        _deleteTaskDalProvider = deleteTaskDalProvider;
    }
    
    public async Task<DeleteTaskResult> DeleteTask(Guid taskId, Guid userId, CancellationToken cancellationToken)
    {
        var result = await _deleteTaskDalProvider.DeleteTask(userId, taskId, cancellationToken);
        if (result is null)
        {
            return new DeleteTaskResult
            {
                ErrorCode = ErrorCodes.TaskNotFound
            };
        }

        return new DeleteTaskResult
        {
            TaskId = result.TaskId,
        };
    }
}
