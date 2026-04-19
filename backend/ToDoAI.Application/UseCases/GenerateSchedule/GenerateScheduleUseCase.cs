using ToDoAI.Application.Abstractions.DalProviders.GetTaskDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.GetTaskDalProvider.Models;
using ToDoAI.Application.Abstractions.DalProviders.DayScheduleDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.DayScheduleDalProvider.Models;
using ToDoAI.Application.Abstractions.DalProviders.UserDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.UserStateDalProvider;
using ToDoAI.Application.UseCases.GenerateSchedule.Mappers;
using ToDoAI.Application.UseCases.GenerateSchedule.Models;
using ToDoAI.Domain.Enums;

namespace ToDoAI.Application.UseCases.GenerateSchedule;

public sealed class GenerateScheduleUseCase : IGenerateScheduleUseCase
{
    private readonly IUserStateDalProvider  _userStateDalProvider;
    private readonly IUserDalProvider  _userDalProvider;
    private readonly IGetTaskDalProvider _getTaskDalProvider;
    private readonly IDayScheduleDalProvider _dayScheduleDalProvider;
    
    public GenerateScheduleUseCase(
        IUserStateDalProvider userStateDalProvider,
        IUserDalProvider userDalProvider,
        IGetTaskDalProvider getTaskDalProvider,
        IDayScheduleDalProvider dayScheduleDalProvider)
    {
        _userStateDalProvider = userStateDalProvider;
        _userDalProvider = userDalProvider;
        _getTaskDalProvider = getTaskDalProvider;
        _dayScheduleDalProvider = dayScheduleDalProvider;
    }

    public async Task<GenerateScheduleBlResult> GenerateSchedule(GenerateScheduleBlRequest request, CancellationToken cancellationToken)
    {
        var user = await _userDalProvider.GetUser(request.UserId, cancellationToken);

        if (user == null)
        {
            return new GenerateScheduleBlResult
            {
                ErrorCode = ErrorCodes.NotAuthorized
            };
        }

        var uniqueTaskIds = request.TaskIds.Distinct().ToArray();
        if (uniqueTaskIds.Length == 0)
        {
            return new GenerateScheduleBlResult
            {
                ErrorCode = ErrorCodes.IncorrectValue
            };
        }

        var tasksList = new List<TaskDal>(uniqueTaskIds.Length);
        foreach (var taskId in uniqueTaskIds)
        {
            var task = await _getTaskDalProvider.GetTask(taskId, request.UserId, cancellationToken);
            if (task is null)
            {
                return new GenerateScheduleBlResult
                {
                    ErrorCode = ErrorCodes.TaskNotFound
                };
            }

            if (task.WorkStatus is WorkStatus.Completed or WorkStatus.Running)
            {
                return new GenerateScheduleBlResult
                {
                    ErrorCode = ErrorCodes.IncorrectValue
                };
            }

            tasksList.Add(task);
        }

        var userState = await _userStateDalProvider.GetLatestUserState(request.UserId, cancellationToken);
        if (userState == null || DateOnly.FromDateTime(userState.CreatedAt.DateTime) != request.ScheduleDate)
        {
            return new GenerateScheduleBlResult
            {
                ErrorCode = ErrorCodes.UserStateNotFound
            };
        }

        var orderedTasks = tasksList
            .OrderByDescending(x => x.Priority)
            .ThenBy(x => x.DeadlineAt)
            .ThenBy(x => x.CreatedAt)
            .ToArray();

        var daySchedule = await _dayScheduleDalProvider.CreateDaySchedule(new ScheduleDalRequest
        {
            UserId = request.UserId,
            ScheduleDate = request.ScheduleDate,
            StartAt = request.StartAt,
            TaskList = orderedTasks
        }, cancellationToken);


        DayScheduleBlResult dayScheduleBlResult = daySchedule.ToDayScheduleBl();
        return new GenerateScheduleBlResult
        {
            DaySchedule = dayScheduleBlResult
        };
    }
}
