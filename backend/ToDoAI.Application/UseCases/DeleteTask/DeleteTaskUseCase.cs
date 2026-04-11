using ToDoAI.Application.Abstractions.DalProviders.DeleteTaskDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.UserDalProvider;
using ToDoAI.Application.UseCases.DeleteTask.Models;
using ToDoAI.Domain.Enums;

namespace ToDoAI.Application.UseCases.DeleteTask;

public sealed class DeleteTaskUseCase : IDeleteTaskUseCase
{
    private readonly IDeleteTaskDalProvider _deleteTaskDalProvider;
    private readonly IUserDalProvider _userDalProvider;

    public DeleteTaskUseCase(IDeleteTaskDalProvider deleteTaskDalProvider, IUserDalProvider userDalProvider)
    {
        _deleteTaskDalProvider = deleteTaskDalProvider;
        _userDalProvider = userDalProvider;
    }
    
    public async Task<DeleteTaskResult> DeleteTask(Guid taskId, Guid userId, CancellationToken cancellationToken)
    {
        var user = await _userDalProvider.GetUser(userId, cancellationToken);

        if (user == null)
        {
            return new DeleteTaskResult
            {
                ErrorCode = ErrorCodes.NotAuthorized
            };
        }
        
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
