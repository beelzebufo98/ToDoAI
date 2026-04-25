using ToDoAI.Application.Abstractions.DalProviders.GetTaskDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.ScheduleDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.TaskWorkSessionDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.TaskWorkSessionDalProvider.Models;
using ToDoAI.Application.Abstractions.DalProviders.UpdateTaskStatusDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.UpdateTaskStatusDalProvider.Models;
using ToDoAI.Application.Abstractions.DalProviders.UserDalProvider;
using ToDoAI.Application.UseCases.TaskWorkSession.Models;
using ToDoAI.Domain.Enums;

namespace ToDoAI.Application.UseCases.TaskWorkSession;

public sealed class CreateTaskWorkSessionUseCase : ICreateTaskWorkSessionUseCase
{
    private readonly IUserDalProvider  _userDalProvider;
    private readonly IGetTaskDalProvider  _getTaskDalProvider;
    private readonly IScheduleDalProvider  _scheduleDalProvider;
    private readonly ITaskWorkSessionDalProvider _taskWorkSessionDalProvider;
    private readonly IUpdateTaskStatusDalProvider _updateTaskStatusDalProvider;

    public CreateTaskWorkSessionUseCase(
        IUserDalProvider userDalProvider, 
        IGetTaskDalProvider getTaskDalProvider,
        IScheduleDalProvider scheduleDalProvider,
        ITaskWorkSessionDalProvider taskWorkSessionDalProvider,
        IUpdateTaskStatusDalProvider updateTaskStatusDalProvider)
    {
        _userDalProvider = userDalProvider;
        _getTaskDalProvider = getTaskDalProvider;
        _scheduleDalProvider = scheduleDalProvider;
        _taskWorkSessionDalProvider = taskWorkSessionDalProvider;
        _updateTaskStatusDalProvider = updateTaskStatusDalProvider;
    }

    public async Task<TaskWorkSessionBlResult> CreateTaskWorkSession(CreateWorkSessionBlRequest request, CancellationToken cancellationToken)
    {
        var user = await _userDalProvider.GetUser(request.UserId, cancellationToken);

        if (user == null)
        {
            return new TaskWorkSessionBlResult
            {
                ErrorCode = ErrorCodes.NotAuthorized
            };
        }
        
        var taskResult = await _getTaskDalProvider.GetTask(request.TaskId, request.UserId, cancellationToken);
        if (taskResult is null)
        {
            return new TaskWorkSessionBlResult
            {
                ErrorCode = ErrorCodes.TaskNotFound
            };
        }

        if (taskResult.WorkStatus is WorkStatus.Completed or WorkStatus.Deleted)
        {
            return new TaskWorkSessionBlResult
            {
                ErrorCode = ErrorCodes.InvalidTaskStatusTransition
            };
        }
        
        if (request.ScheduleId is not null)
        {
            var scheduleDal = await _scheduleDalProvider.GetSchedule(request.ScheduleId.Value,
                request.UserId, cancellationToken);
            if (scheduleDal is null)
            {
                return new TaskWorkSessionBlResult
                {
                    ErrorCode = ErrorCodes.ScheduleNotFound
                };
            }

            if (scheduleDal.TaskId != request.TaskId)
            {
                return new TaskWorkSessionBlResult
                {
                    ErrorCode = ErrorCodes.ScheduleDoesNotMatchTask
                };
            }
        }

        var sessionRequest = new CreateWorkSessionDalRequest
        {
            UserId = request.UserId,
            TaskId = request.TaskId,
            ScheduleId = request.ScheduleId,
        };
        
        var session = await _taskWorkSessionDalProvider.CreateTaskWorkSession(sessionRequest, cancellationToken);
        if (session is null)
        {
            return new TaskWorkSessionBlResult
            {
                ErrorCode = ErrorCodes.TaskWorkSessionAlreadyExists
            };
        }

        if (taskResult.WorkStatus != WorkStatus.Running)
        {
            var updateTaskStatusResult = await _updateTaskStatusDalProvider.UpdateTaskStatus(
                new UpdateTaskStatusDalRequest
                {
                    UserId = request.UserId,
                    TaskId = request.TaskId,
                    WorkStatus = TaskWorkStatusDal.Running,
                },
                cancellationToken);

            if (updateTaskStatusResult is null || updateTaskStatusResult.ErrorCode is not null)
            {
                return new TaskWorkSessionBlResult
                {
                    ErrorCode = updateTaskStatusResult?.ErrorCode ?? ErrorCodes.TaskNotFound
                };
            }
        }

        var result = new SessionBlResult
        {
            SessionId = session.SessionId,
            UserId = session.UserId,
            TaskId = session.TaskId,
            ScheduleBlockId = session.ScheduleBlockId,
            StartedAt = session.StartedAt,
            EndedAt = session.EndedAt,
            SpentMinutes = session.SpentMinutes,
            Source = session.Source,
            Status = session.Status,
            CreatedAt = session.CreatedAt,
        };
        
        return new TaskWorkSessionBlResult
        {
            SessionBlResult = result
        };
    }
}
