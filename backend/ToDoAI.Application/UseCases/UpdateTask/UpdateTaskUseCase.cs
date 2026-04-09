using ToDoAI.Application.Abstractions.DalProviders.UpdateTaskDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.UpdateTaskDalProvider.Models;
using ToDoAI.Application.UseCases.GetTask.Mappers;
using ToDoAI.Application.UseCases.GetTask.Models;
using ToDoAI.Application.UseCases.UpdateTask.Models;
using ToDoAI.Domain.Enums;

namespace ToDoAI.Application.UseCases.UpdateTask;

public sealed class UpdateTaskUseCase : IUpdateTaskUseCase
{
    private readonly IUpdateTaskDalProvider _updateTaskDalProvider;

    public UpdateTaskUseCase(IUpdateTaskDalProvider updateTaskDalProvider)
    {
        _updateTaskDalProvider = updateTaskDalProvider;
    }

    public async Task<GetTaskResult> UpdateTask(UpdateTaskBlRequest request, CancellationToken cancellationToken)
    {
        var taskReq = new UpdateTaskDalRequest
        {
            TaskId = request.TaskId,
            UserId = request.UserId,
            Title = request.Title,
            Description = request.Description,
            EstimatedMinutes = request.EstimatedMinutes,
            Priority = request.Priority,
            ComplexityLevel = request.ComplexityLevel,
            DeadlineAt = request.DeadlineAt
        };
        
        var result = await _updateTaskDalProvider.UpdateTask(taskReq, cancellationToken);

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
}
