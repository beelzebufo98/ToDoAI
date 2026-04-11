using ToDoAI.Application.Abstractions.DalProviders.GetTaskDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.GetTaskDalProvider.Models;
using ToDoAI.Application.Abstractions.DalProviders.UserDalProvider;
using ToDoAI.Application.UseCases.GetTask.Mappers;
using ToDoAI.Application.UseCases.GetTask.Models;
using ToDoAI.Domain.Enums;

namespace ToDoAI.Application.UseCases.GetTask;

public sealed class GetTaskUseCase : IGetTaskUseCase
{
    private readonly IGetTaskDalProvider _getTaskDalProvider;
    private readonly IUserDalProvider _userDalProvider;

    public GetTaskUseCase(IGetTaskDalProvider getTaskDalProvider, IUserDalProvider userDalProvider)
    {
        _getTaskDalProvider = getTaskDalProvider;
        _userDalProvider = userDalProvider;
    }
    
    public async Task<GetTaskResult> GetTask(Guid taskId, Guid userId, CancellationToken cancellationToken)
    {
        var user = await _userDalProvider.GetUser(userId, cancellationToken);

        if (user == null)
        {
            return new GetTaskResult
            {
                ErrorCode = ErrorCodes.NotAuthorized
            };
        }
        
        var result = await _getTaskDalProvider.GetTask(taskId, userId, cancellationToken);
        if (result is null)
        {
            return new GetTaskResult
            {
                ErrorCode = ErrorCodes.TaskNotFound
            };
        }
        
        var task = result.GetTaskResult();
        return new GetTaskResult
        {
            TaskResult = task
        };
    }

    public async Task<GetTasksResult> GetTasks(Guid userId, TaskFiltersBlRequest filters, CancellationToken cancellationToken)
    {
        var result = await _getTaskDalProvider.GetTasks(userId, filters, cancellationToken);
        var taskList = result.Select(x => x.GetTaskResult()).ToList();
        return new GetTasksResult
        {
            TaskResults = taskList
        };
    }
}
