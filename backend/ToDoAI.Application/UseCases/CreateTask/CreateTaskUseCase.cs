using ToDoAI.Application.Abstractions.DalProviders.CreateTaskDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.CreateTaskDalProvider.Models;
using ToDoAI.Application.Abstractions.DalProviders.UserDalProvider;
using ToDoAI.Application.UseCases.CreateTask.Models;
using ToDoAI.Domain.Enums;

namespace ToDoAI.Application.UseCases.CreateTask;

public sealed class CreateTaskUseCase :  ICreateTaskUseCase
{
    private readonly IUserDalProvider _userDalProvider;
    private readonly ICreateTaskDalProvider _createTaskDalProvider;

    public CreateTaskUseCase(IUserDalProvider userDalProvider, ICreateTaskDalProvider createTaskDalProvider)
    {
        _userDalProvider = userDalProvider;
        _createTaskDalProvider = createTaskDalProvider;
    }
    public async Task<CreateTaskResult> CreateTask(CreateTaskBlRequest task, CancellationToken cancellationToken)
    {
        var user = await _userDalProvider.GetUser(task.UserId, cancellationToken);

        if (user == null)
        {
            return new CreateTaskResult
            {
                ErrorCode = ErrorCodes.NotAuthorized
            };
        }

        var taskRequest = new CreateTaskReqDal
        {
            TaskId = Guid.NewGuid(),
            Title = task.Title,
            Description = task.Description,
            EstimatedMinutes = task.EstimatedMinutes,
            ComplexityLevel = task.ComplexityLevel,
            PriorityLevel = task.Priority,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            UserId = user.UserId
        };
        var result = await _createTaskDalProvider.CreateTask(taskRequest, cancellationToken);
        return new CreateTaskResult
        {
            TaskId = result.TaskId
        };
    }
}
