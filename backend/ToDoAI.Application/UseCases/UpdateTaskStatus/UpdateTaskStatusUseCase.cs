using ToDoAI.Application.Abstractions.DalProviders.UpdateTaskStatusDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.UpdateTaskStatusDalProvider.Models;
using ToDoAI.Application.Abstractions.DalProviders.UserDalProvider;
using ToDoAI.Application.UseCases.GetTask.Models;
using ToDoAI.Application.UseCases.UpdateTaskStatus.Models;
using ToDoAI.Domain.Enums;

namespace ToDoAI.Application.UseCases.UpdateTaskStatus;

public sealed class UpdateTaskStatusUseCase : IUpdateTaskStatusUseCase
{
    private readonly IUpdateTaskStatusDalProvider  _updateTaskStatusDalProvider;
    private readonly IUserDalProvider _userDalProvider;

    public UpdateTaskStatusUseCase(IUpdateTaskStatusDalProvider updateTaskStatusDalProvider, IUserDalProvider userDalProvider)
    {
        _updateTaskStatusDalProvider = updateTaskStatusDalProvider;
        _userDalProvider = userDalProvider;
    }
    
    public async Task<UpdateTaskStatusResult> UpdateTaskStatus(UpdateTaskStatusBlRequest updateTaskStatusBlRequest,
        CancellationToken cancellationToken)
    {
        var user = await _userDalProvider.GetUser(updateTaskStatusBlRequest.UserId, cancellationToken);

        if (user == null)
        {
            return new UpdateTaskStatusResult
            {
                ErrorCode = ErrorCodes.NotAuthorized
            };
        }
        
        var request = new UpdateTaskStatusDalRequest
        {
            UserId = updateTaskStatusBlRequest.UserId,
            TaskId = updateTaskStatusBlRequest.TaskId,
            WorkStatus = GetTaskWorkStatusDal(updateTaskStatusBlRequest.TaskWorkStatus),
        };
        var result = await _updateTaskStatusDalProvider.UpdateTaskStatus(request, cancellationToken);

        if (result is null)
        {
            return new UpdateTaskStatusResult
            {
                ErrorCode = ErrorCodes.TaskNotFound
            }; 
        }

        if (result.ErrorCode is not null)
        {
            return new UpdateTaskStatusResult
            {
                ErrorCode = result.ErrorCode
            };
        }

        return new UpdateTaskStatusResult
        {
            WorkStatus = GetTaskWorkStatusBl(result.WorkStatus),
        };

    }

    private TaskWorkStatusDal GetTaskWorkStatusDal(TaskWorkStatusBlFilters workStatus) => workStatus switch
    {
        TaskWorkStatusBlFilters.New => TaskWorkStatusDal.New,
        TaskWorkStatusBlFilters.Todo => TaskWorkStatusDal.Todo,
        TaskWorkStatusBlFilters.Running => TaskWorkStatusDal.Running,
        TaskWorkStatusBlFilters.Completed => TaskWorkStatusDal.Completed,
        _ => throw new ArgumentOutOfRangeException(nameof(workStatus), workStatus, null)
    };

    private TaskWorkStatusBlFilters GetTaskWorkStatusBl(TaskWorkStatusDal workStatus) => workStatus switch
    {
        TaskWorkStatusDal.New => TaskWorkStatusBlFilters.New,
        TaskWorkStatusDal.Todo => TaskWorkStatusBlFilters.Todo,
        TaskWorkStatusDal.Running => TaskWorkStatusBlFilters.Running,
        TaskWorkStatusDal.Completed => TaskWorkStatusBlFilters.Completed,
        _ => throw new ArgumentOutOfRangeException(nameof(workStatus), workStatus, null)
    };
}
