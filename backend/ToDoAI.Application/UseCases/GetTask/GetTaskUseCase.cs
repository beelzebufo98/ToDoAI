using ToDoAI.Application.Abstractions.DalProviders.GetTaskDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.GetTaskDalProvider.Models;
using ToDoAI.Application.UseCases.GetTask.Models;
using ToDoAI.Domain.Enums;

namespace ToDoAI.Application.UseCases.GetTask;

public sealed class GetTaskUseCase : IGetTaskUseCase
{
    private readonly IGetTaskDalProvider _getTaskDalProvider;

    public GetTaskUseCase(IGetTaskDalProvider getTaskDalProvider)
    {
        _getTaskDalProvider = getTaskDalProvider;
    }
    
    public async Task<GetTaskResult> GetTask(Guid taskId, Guid userId, CancellationToken cancellationToken)
    {
        var result = await _getTaskDalProvider.GetTask(taskId, userId, cancellationToken);
        if (result is null)
        {
            return new GetTaskResult
            {
                ErrorCode = ErrorCodes.TaskNotFound
            };
        }
        
        var task = GetTaskResult(result);
        return new GetTaskResult
        {
            TaskResult = task
        };
    }

    public async Task<GetTasksResult> GetTasks(Guid userId, TaskFiltersBlRequest filters, CancellationToken cancellationToken)
    {
        var result = await _getTaskDalProvider.GetTasks(userId, filters, cancellationToken);
        var taskList = result.Select(GetTaskResult).ToList();
        return new GetTasksResult
        {
            TaskResults = taskList
        };
    }

    private TaskResult GetTaskResult(TaskDal taskDal)
    {
        return new TaskResult
        {
            Id = taskDal.Id,
            Title = taskDal.Title,
            Description = taskDal.Description,
            EstimatedMinutes = taskDal.EstimatedMinutes,
            ComplexityLevel = taskDal.ComplexityLevel,
            Priority = taskDal.Priority,
            CreatedAt = taskDal.CreatedAt,
            UpdatedAt = taskDal.UpdatedAt,
            ActualStartDate = taskDal.ActualStartDate,
            ActualEndDate = taskDal.ActualEndDate,
            WorkStatus = taskDal.WorkStatus
        };
    }
}
