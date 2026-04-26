using ToDoAI.Application.Abstractions.DalProviders.GetTaskDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.ScheduleDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.TaskExecutionDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.TaskExecutionDalProvider.Models;
using ToDoAI.Application.Abstractions.DalProviders.UserDalProvider;
using ToDoAI.Application.Abstractions.Services.AiService;
using ToDoAI.Application.Abstractions.Services.AiService.Models;
using ToDoAI.Application.UseCases.TaskExecutionUseCase.Models;
using ToDoAI.Domain.Enums;

namespace ToDoAI.Application.UseCases.TaskExecutionUseCase;

public sealed class TaskExecutionUseCase : ITaskExecutionUseCase
{
    private readonly ITaskExecutionDalProvider  _taskExecutionDalProvider;
    private readonly IUserDalProvider _userDalProvider;
    private readonly IGetTaskDalProvider _getTaskDalProvider;
    private readonly IScheduleDalProvider  _scheduleDalProvider;
    private readonly IAiMotivationClient _aiMotivationClient;

    public TaskExecutionUseCase(ITaskExecutionDalProvider taskExecutionDalProvider,
        IUserDalProvider userDalProvider,
        IGetTaskDalProvider getTaskDalProvider,
        IScheduleDalProvider scheduleDalProvider,
        IAiMotivationClient aiMotivationClient)
    {
        _taskExecutionDalProvider = taskExecutionDalProvider;
        _userDalProvider = userDalProvider;
        _getTaskDalProvider = getTaskDalProvider;
        _scheduleDalProvider = scheduleDalProvider;
        _aiMotivationClient = aiMotivationClient;
    }

    public async Task<TaskExecutionBlResult> CreateTaskExecution(TaskExecutionBlRequest taskExecutionBlRequest,
        CancellationToken cancellationToken)
    {
        var user = await _userDalProvider.GetUser(taskExecutionBlRequest.UserId, cancellationToken);

        if (user == null)
        {
            return new TaskExecutionBlResult
            {
                ErrorCode = ErrorCodes.NotAuthorized
            };
        }
        
        var taskResult = await _getTaskDalProvider.GetTask(taskExecutionBlRequest.TaskId, taskExecutionBlRequest.UserId, cancellationToken);
        if (taskResult is null)
        {
            return new TaskExecutionBlResult
            {
                ErrorCode = ErrorCodes.TaskNotFound
            };
        }

        if (taskResult.WorkStatus != WorkStatus.Completed)
        {
            return new TaskExecutionBlResult
            {
                ErrorCode = ErrorCodes.TaskShouldBeCompleted
            };
        }
        
        if (taskExecutionBlRequest.ScheduleId is not null)
        {
            var scheduleDal = await _scheduleDalProvider.GetSchedule(taskExecutionBlRequest.ScheduleId.Value,
                taskExecutionBlRequest.UserId, cancellationToken);
            if (scheduleDal is null)
            {
                return new TaskExecutionBlResult
                {
                    ErrorCode = ErrorCodes.ScheduleNotFound
                };
            }

            if (scheduleDal.TaskId != taskExecutionBlRequest.TaskId)
            {
                return new TaskExecutionBlResult
                {
                    ErrorCode = ErrorCodes.ScheduleDoesNotMatchTask
                };
            }
        }

        var dalRequest = new TaskExecutionDalRequest
        {
            TaskId = taskExecutionBlRequest.TaskId,
            ScheduleId = taskExecutionBlRequest.ScheduleId,
            ActualMinutes = taskExecutionBlRequest.ActualMinutes,
            EnergyAfter = taskExecutionBlRequest.EnergyAfter,
            StressAfter = taskExecutionBlRequest.StressAfter,
            CreatedAt = DateTimeOffset.UtcNow
        };
        
        var result = await _taskExecutionDalProvider.CreateTaskExecution(dalRequest, cancellationToken);
        if (result is null)
        {
            return new TaskExecutionBlResult
            {
                ErrorCode = ErrorCodes.TaskExecutionAlreadyExists
            };
        }

        var motivationMessage = await GetTaskCompletionMotivationMessage(cancellationToken);

        return new TaskExecutionBlResult
        {
            TaskExecutionResult = new TaskExecutionResult
            {
                TaskExecutionId = result.TaskExecutionId,
                TaskId = result.TaskId,
                ScheduleId = result.ScheduleId,
                ActualMinutes = result.ActualMinutes,
                EnergyAfter = result.EnergyAfter,
                StressAfter = result.StressAfter,
                CreatedAt = result.CreatedAt,
                MotivationMessage = motivationMessage
            }
        };

    }

    private async Task<string> GetTaskCompletionMotivationMessage(CancellationToken cancellationToken)
    {
        var aiResult = await _aiMotivationClient.GenerateMotivation(
            new AiGenerateMotivationRequest
            {
                Trigger = "task_completion"
            },
            cancellationToken);

        var message = aiResult.Response?.Message?.Trim();
        if (!string.IsNullOrWhiteSpace(message))
        {
            return message;
        }

        return "Задача закрыта. Это уже реальный прогресс, а не только план.";
    }
}