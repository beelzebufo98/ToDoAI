using ToDoAI.Application.Abstractions.DalProviders.GetTaskDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.GetTaskDalProvider.Models;
using ToDoAI.Application.Abstractions.DalProviders.DayScheduleDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.DayScheduleDalProvider.Models;
using ToDoAI.Application.Abstractions.DalProviders.TaskWorkSessionDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.TaskExecutionDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.UserDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.UserStateDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.UserStateDalProvider.Models;
using ToDoAI.Application.Abstractions.Services.AiService;
using ToDoAI.Application.Abstractions.Services.AiService.Models;
using ToDoAI.Application.Common;
using ToDoAI.Application.UseCases.GenerateSchedule.Mappers;
using ToDoAI.Application.UseCases.GenerateSchedule.Models;
using ToDoAI.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace ToDoAI.Application.UseCases.GenerateSchedule;

public sealed class GenerateScheduleUseCase : IGenerateScheduleUseCase
{
    private static readonly TimeSpan UserLocalOffset = TimeSpan.FromHours(3);
    private readonly IUserStateDalProvider  _userStateDalProvider;
    private readonly IUserDalProvider  _userDalProvider;
    private readonly IGetTaskDalProvider _getTaskDalProvider;
    private readonly IDayScheduleDalProvider _dayScheduleDalProvider;
    private readonly ITaskWorkSessionDalProvider _taskWorkSessionDalProvider;
    private readonly ITaskExecutionDalProvider _taskExecutionDalProvider;
    private readonly IAiScheduleClient _aiScheduleClient;
    private readonly ILogger<GenerateScheduleUseCase> _logger;
    
    public GenerateScheduleUseCase(
        IUserStateDalProvider userStateDalProvider,
        IUserDalProvider userDalProvider,
        IGetTaskDalProvider getTaskDalProvider,
        IDayScheduleDalProvider dayScheduleDalProvider,
        ITaskWorkSessionDalProvider taskWorkSessionDalProvider,
        ITaskExecutionDalProvider taskExecutionDalProvider,
        IAiScheduleClient aiScheduleClient,
        ILogger<GenerateScheduleUseCase> logger)
    {
        _userStateDalProvider = userStateDalProvider;
        _userDalProvider = userDalProvider;
        _getTaskDalProvider = getTaskDalProvider;
        _dayScheduleDalProvider = dayScheduleDalProvider;
        _taskWorkSessionDalProvider = taskWorkSessionDalProvider;
        _taskExecutionDalProvider = taskExecutionDalProvider;
        _aiScheduleClient = aiScheduleClient;
        _logger = logger;
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

        var (scheduledTasks, unscheduledTasks, explanations) = await BuildScheduleWithFallback(
            request,
            tasksList,
            userState,
            cancellationToken);

        var daySchedule = await _dayScheduleDalProvider.CreateDaySchedule(new ScheduleDalRequest
        {
            UserId = request.UserId,
            ScheduleDate = request.ScheduleDate,
            StartAt = request.StartAt,
            TaskList = scheduledTasks
        }, cancellationToken);

        DayScheduleBlResult dayScheduleBlResult = daySchedule.ToDayScheduleBl() with
        {
            Explanations = explanations
        };
        return new GenerateScheduleBlResult
        {
            DaySchedule = dayScheduleBlResult,
            Unscheduled = unscheduledTasks
        };
    }

    private async Task<(List<TaskDal> ScheduledTasks, List<UnscheduledTaskBlResult> UnscheduledTasks, IReadOnlyCollection<string> Explanations)> BuildScheduleWithFallback(
        GenerateScheduleBlRequest request,
        List<TaskDal> tasksList,
        UserStateDal userState,
        CancellationToken cancellationToken)
    {
        var aiResult = await TryBuildScheduleWithAi(request, tasksList, userState, cancellationToken);
        if (aiResult.ScheduledTasks is not null)
        {
            _logger.LogInformation(
                "Schedule generation completed. used_ai={UsedAi}, fallback_reason={FallbackReason}",
                aiResult.UsedAi,
                aiResult.FallbackReason);
            return (aiResult.ScheduledTasks, aiResult.UnscheduledTasks!, aiResult.Explanations);
        }

        var fallbackResult = BuildScheduleDeterministically(request, tasksList);
        _logger.LogInformation(
            "Schedule generation completed. used_ai={UsedAi}, fallback_reason={FallbackReason}",
            false,
            aiResult.FallbackReason);
        return (
            fallbackResult.ScheduledTasks,
            fallbackResult.UnscheduledTasks,
            ["План построен по приоритетам, дедлайнам и оставшемуся времени."]
        );
    }

    private async Task<(bool UsedAi, string? FallbackReason, List<TaskDal>? ScheduledTasks, List<UnscheduledTaskBlResult>? UnscheduledTasks, IReadOnlyCollection<string> Explanations)> TryBuildScheduleWithAi(
        GenerateScheduleBlRequest request,
        List<TaskDal> tasksList,
        UserStateDal userState,
        CancellationToken cancellationToken)
    {
        var taskMap = tasksList.ToDictionary(x => x.Id);
        var recentExecutions = await _taskExecutionDalProvider.GetRecentTaskExecutions(
            request.UserId,
            count: 5,
            cancellationToken);

        _logger.LogInformation(
            "Sending schedule generation request to AI. schedule_date={ScheduleDate}, day_start_at={DayStartAt}, tasks_count={TasksCount}, recent_executions_count={RecentExecutionsCount}",
            request.ScheduleDate,
            request.StartAt,
            tasksList.Count,
            recentExecutions.Count);

        var aiRequest = new AiGenerateScheduleRequest
        {
            ScheduleDate = request.ScheduleDate,
            DayStartAt = request.StartAt,
            DayEndAt = new DateTimeOffset(request.ScheduleDate.ToDateTime(TimeOnly.MinValue), request.StartAt.Offset)
                .AddDays(1),
            UserState = new AiUserStateRequest
            {
                SleepMinutes = userState.SleepMinutes,
                EnergyLevel = userState.EnergyLevel,
                StressLevel = userState.StressLevel,
                MotivationLevel = userState.MotivationLevel,
                ConcentrationLevel = userState.ConcentrationLevel,
            },
            RecentExecutions = recentExecutions.Select(execution => new AiRecentTaskExecutionRequest
            {
                TaskId = execution.TaskId,
                TaskTitle = execution.TaskTitle,
                EstimatedMinutes = execution.TaskEstimatedMinutes,
                ActualMinutes = execution.ActualMinutes,
                Priority = execution.TaskPriority,
                ComplexityLevel = execution.TaskComplexityLevel,
                EnergyAfter = execution.EnergyAfter,
                StressAfter = execution.StressAfter,
                CreatedAt = execution.CreatedAt,
            }).ToArray(),
            Tasks = tasksList.Select(task => new AiPlanningTaskRequest
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                EstimatedMinutes = task.EstimatedMinutes,
                RemainingMinutes = TaskTimeCalculator.CalculateRemainingMinutes(
                    task.EstimatedMinutes,
                    task.ActualSpentMinutes,
                    task.WorkStatus),
                Priority = task.Priority,
                ComplexityLevel = task.ComplexityLevel,
                DeadlineAt = task.DeadlineAt,
                WorkStatus = task.WorkStatus,
            }).ToArray()
        };

        var aiResult = await _aiScheduleClient.GenerateSchedule(aiRequest, cancellationToken);
        if (!aiResult.UsedAi || aiResult.Response is null)
        {
            _logger.LogWarning(
                "AI schedule request did not produce a usable response. fallback_reason={FallbackReason}",
                aiResult.FallbackReason);
            return (false, aiResult.FallbackReason, null, null, []);
        }

        _logger.LogInformation(
            "AI schedule response received. scheduled_count={ScheduledCount}, unscheduled_count={UnscheduledCount}, explanations_count={ExplanationsCount}, planner_model={PlannerModel}, used_fallback_ranking={UsedFallbackRanking}",
            aiResult.Response.Scheduled.Count,
            aiResult.Response.Unscheduled.Count,
            aiResult.Response.Summary?.Explanations.Count ?? 0,
            aiResult.Response.Summary?.PlannerModel,
            aiResult.Response.Summary?.UsedFallbackRanking);

        var scheduledTasks = new List<TaskDal>(aiResult.Response.Scheduled.Count);
        foreach (var block in aiResult.Response.Scheduled.OrderBy(x => x.StartAt))
        {
            if (!taskMap.TryGetValue(block.TaskId, out var task))
            {
                _logger.LogWarning("AI returned unknown TaskId {TaskId}. Falling back to deterministic schedule generation.", block.TaskId);
                return (false, "unknown_task_id", null, null, []);
            }

            scheduledTasks.Add(task with
            {
                EstimatedMinutes = block.PlannedMinutes
            });
        }

        var unscheduledTasks = aiResult.Response.Unscheduled
            .Select(item =>
            {
                taskMap.TryGetValue(item.TaskId, out var task);
                return new UnscheduledTaskBlResult
                {
                TaskId = item.TaskId,
                TaskTitle = task?.Title ?? item.Title,
                Description = task?.Description ?? string.Empty,
                EstimatedMinutes = item.RemainingMinutes
                };
            })
            .ToList();

        return (true, null, scheduledTasks, unscheduledTasks, aiResult.Response.Summary?.Explanations ?? []);
    }

    private static (List<TaskDal> ScheduledTasks, List<UnscheduledTaskBlResult> UnscheduledTasks) BuildScheduleDeterministically(
        GenerateScheduleBlRequest request,
        List<TaskDal> tasksList)
    {
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

        return (scheduledTasks, unscheduledTasks);
    }
}
