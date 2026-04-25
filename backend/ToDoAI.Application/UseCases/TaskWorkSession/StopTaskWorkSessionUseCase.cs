using ToDoAI.Application.Abstractions.DalProviders.TaskWorkSessionDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.UserDalProvider;
using ToDoAI.Application.UseCases.TaskWorkSession.Models;
using ToDoAI.Domain.Enums;

namespace ToDoAI.Application.UseCases.TaskWorkSession;

public sealed class StopTaskWorkSessionUseCase : IStopTaskWorkSessionUseCase
{
    private readonly IUserDalProvider  _userDalProvider;
    private readonly ITaskWorkSessionDalProvider  _taskWorkSessionDalProvider;
    
    public StopTaskWorkSessionUseCase(IUserDalProvider userDalProvider, ITaskWorkSessionDalProvider taskWorkSessionDalProvider)
    {
        _userDalProvider = userDalProvider;
        _taskWorkSessionDalProvider = taskWorkSessionDalProvider;
    }

    public async Task<TaskWorkSessionBlResult> StopTaskWorkSession(Guid userId, Guid sessionId,
        CancellationToken cancellationToken)
    {
        var user = await _userDalProvider.GetUser(userId, cancellationToken);

        if (user == null)
        {
            return new TaskWorkSessionBlResult
            {
                ErrorCode = ErrorCodes.NotAuthorized
            };
        }
        
        var stopResult = await _taskWorkSessionDalProvider.StopTaskWorkSessionAsync(userId, sessionId, cancellationToken);

        if (stopResult.ErrorCode is not null)
        {
            return new TaskWorkSessionBlResult
            {
                ErrorCode = stopResult.ErrorCode
            }; 
        }

        var session = stopResult.Session!;
        
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
            TaskEstimatedMinutes = session.TaskEstimatedMinutes,
            TaskActualSpentMinutes = session.TaskActualSpentMinutes,
        };
        
        return new TaskWorkSessionBlResult
        {
            SessionBlResult = result
        };
    }
}
