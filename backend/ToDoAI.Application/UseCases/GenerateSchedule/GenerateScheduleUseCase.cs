using ToDoAI.Application.Abstractions.DalProviders.GetTaskDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.GetTaskDalProvider.Models;
using ToDoAI.Application.Abstractions.DalProviders.DayScheduleDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.DayScheduleDalProvider.Models;
using ToDoAI.Application.Abstractions.DalProviders.TaskWorkSessionDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.UserDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.UserStateDalProvider;
using ToDoAI.Application.Common;
using ToDoAI.Application.UseCases.GenerateSchedule.Mappers;
using ToDoAI.Application.UseCases.GenerateSchedule.Models;
using ToDoAI.Domain.Enums;

namespace ToDoAI.Application.UseCases.GenerateSchedule;

public sealed class GenerateScheduleUseCase : IGenerateScheduleUseCase
{
    private static readonly TimeSpan UserLocalOffset = TimeSpan.FromHours(3);
    private readonly IUserStateDalProvider  _userStateDalProvider;
    private readonly IUserDalProvider  _userDalProvider;
    private readonly IGetTaskDalProvider _getTaskDalProvider;
    private readonly IDayScheduleDalProvider _dayScheduleDalProvider;
    private readonly ITaskWorkSessionDalProvider _taskWorkSessionDalProvider;
    
    public GenerateScheduleUseCase(
        IUserStateDalProvider userStateDalProvider,
        IUserDalProvider userDalProvider,
        IGetTaskDalProvider getTaskDalProvider,
        IDayScheduleDalProvider dayScheduleDalProvider,
        ITaskWorkSessionDalProvider taskWorkSessionDalProvider)
    {
        _userStateDalProvider = userStateDalProvider;
        _userDalProvider = userDalProvider;
        _getTaskDalProvider = getTaskDalProvider;
        _dayScheduleDalProvider = dayScheduleDalProvider;
        _taskWorkSessionDalProvider = taskWorkSessionDalProvider;
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

        var activeSession = await _taskWorkSessionDalProvider.GetTaskWorkSession(request.UserId, cancellationToken);
        if (activeSession is not null)
        {
            return new GenerateScheduleBlResult
            {
                ErrorCode = ErrorCodes.TaskWorkSessionAlreadyExists
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

            if (task.WorkStatus is WorkStatus.Completed)
            {
                return new GenerateScheduleBlResult
                {
                    ErrorCode = ErrorCodes.IncorrectValue
                };
            }

            tasksList.Add(task);
        }

        var userState = await _userStateDalProvider.GetLatestUserState(request.UserId, cancellationToken);
        var userStateScheduleDate = userState?.CreatedAt.ToOffset(UserLocalOffset).Date;
        var hasStateForScheduleDate = userStateScheduleDate != null &&
                                      DateOnly.FromDateTime(userStateScheduleDate.Value) == request.ScheduleDate;

        if (userState == null || !hasStateForScheduleDate)
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

        var endOfScheduleDay = new DateTimeOffset(
            request.ScheduleDate.ToDateTime(TimeOnly.MinValue),
            request.StartAt.Offset).AddDays(1);

        var scheduledTasks = new List<TaskDal>(orderedTasks.Length);
        var unscheduledTasks = new List<UnscheduledTaskBlResult>();
        var cursor = request.StartAt;

        foreach (var task in orderedTasks)
        {
            var remainingMinutes = TaskTimeCalculator.CalculateRemainingMinutes(
                task.EstimatedMinutes,
                task.ActualSpentMinutes,
                task.WorkStatus);
            if (remainingMinutes == 0)
            {
                unscheduledTasks.Add(new UnscheduledTaskBlResult
                {
                    TaskId = task.Id,
                    TaskTitle = task.Title,
                    Description = task.Description,
                    EstimatedMinutes = 0
                });
                continue;
            }

            var slotEnd = cursor.AddMinutes(remainingMinutes);
            if (slotEnd <= endOfScheduleDay)
            {
                scheduledTasks.Add(task with { EstimatedMinutes = remainingMinutes });
                cursor = slotEnd;
                continue;
            }

            unscheduledTasks.Add(new UnscheduledTaskBlResult
            {
                TaskId = task.Id,
                TaskTitle = task.Title,
                Description = task.Description,
                EstimatedMinutes = remainingMinutes
            });
        }

        var daySchedule = await _dayScheduleDalProvider.CreateDaySchedule(new ScheduleDalRequest
        {
            UserId = request.UserId,
            ScheduleDate = request.ScheduleDate,
            StartAt = request.StartAt,
            TaskList = scheduledTasks
        }, cancellationToken);


        DayScheduleBlResult dayScheduleBlResult = daySchedule.ToDayScheduleBl();
        return new GenerateScheduleBlResult
        {
            DaySchedule = dayScheduleBlResult,
            Unscheduled = unscheduledTasks
        };
    }
}
