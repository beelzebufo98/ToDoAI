using ToDoAI.Application.Abstractions.DalProviders.TaskWorkSessionDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.UserDalProvider;
using ToDoAI.Application.UseCases.TaskWorkSession.Models;
using ToDoAI.Domain.Enums;

namespace ToDoAI.Application.UseCases.TaskWorkSession;

public sealed class GetTaskWorkSessionUseCase : IGetTaskWorkSessionUseCase
{
    private readonly IUserDalProvider  _userDalProvider;
    private readonly ITaskWorkSessionDalProvider _taskWorkSessionDalProvider;

    public GetTaskWorkSessionUseCase(IUserDalProvider userDalProvider,
        ITaskWorkSessionDalProvider taskWorkSessionDalProvider)
    {
        _userDalProvider = userDalProvider;
        _taskWorkSessionDalProvider = taskWorkSessionDalProvider;
    }

    public async Task<GetAllSessionsByDayBlResult> GetAllSessionsByDay(Guid userId, DateOnly sessionDate,
        CancellationToken ct)
    {
        var user = await _userDalProvider.GetUser(userId, ct);

        if (user == null)
        {
            return new GetAllSessionsByDayBlResult
            {
                ErrorCode = ErrorCodes.NotAuthorized
            };
        }
        
        var sesions = await _taskWorkSessionDalProvider.GetTaskWorkSessions(userId, sessionDate, ct);
        
        var sessionBl = new List<SessionBlResult>(sesions.TaskWorkSessions.Count);

        foreach (var session in sesions.TaskWorkSessions)
        {
            var sessionResul = new SessionBlResult
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
                Title =  session.Title,
                TaskEstimatedMinutes = session.TaskEstimatedMinutes,
                TaskActualSpentMinutes = session.TaskActualSpentMinutes,
            };
            sessionBl.Add(sessionResul);
        }

        return new GetAllSessionsByDayBlResult
        {
            Sessions = sessionBl,
            TotalCompletedSessions = sesions.TotalCompletedSessions,
            TotalSessions = sesions.TotalSessions,
            TotalCompletedMinutes = sesions.TotalCompletedMinutes,
        };
    }

    public async Task<TaskWorkSessionBlResult?> GetTaskWorkSession(Guid userId, CancellationToken ct)
    {
        var user = await _userDalProvider.GetUser(userId, ct);

        if (user == null)
        {
            return new TaskWorkSessionBlResult
            {
                ErrorCode = ErrorCodes.NotAuthorized
            };
        }
        
        var session = await _taskWorkSessionDalProvider.GetTaskWorkSession(userId, ct);

        if (session is null)
        {
            return null;
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
            Title =  session.Title,
            TaskEstimatedMinutes = session.TaskEstimatedMinutes,
            TaskActualSpentMinutes = session.TaskActualSpentMinutes,
        };
        
        return new TaskWorkSessionBlResult
        {
            SessionBlResult = result
        };
    }
}
