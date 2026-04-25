using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ToDoAI.API.Controllers.TaskWorkSessionController.Models;
using ToDoAI.Application.UseCases.TaskWorkSession;
using ToDoAI.Application.UseCases.TaskWorkSession.Models;
using ToDoAI.Domain.Enums;

namespace ToDoAI.API.Controllers.TaskWorkSessionController;

[ApiController]
[Authorize]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/task-work-sessions/")]
public sealed class TaskWorkSessionController : ToDoAiControllerBase
{
    private readonly ICreateTaskWorkSessionUseCase  _createTaskWorkSessionUseCase;
    private readonly IStopTaskWorkSessionUseCase  _stopTaskWorkSessionUseCase;
    private readonly ICancelTaskWorkSessionUseCase _cancelTaskWorkSessionUseCase;
    private readonly IGetTaskWorkSessionUseCase _getTaskWorkSessionUseCase;
    
    public TaskWorkSessionController(ICreateTaskWorkSessionUseCase createTaskWorkSessionUseCase,
        IStopTaskWorkSessionUseCase stopTaskWorkSessionUseCase,
        ICancelTaskWorkSessionUseCase cancelTaskWorkSessionUseCase,
        IGetTaskWorkSessionUseCase getTaskWorkSessionUseCase)
    {
        _createTaskWorkSessionUseCase = createTaskWorkSessionUseCase;
        _stopTaskWorkSessionUseCase = stopTaskWorkSessionUseCase;
        _cancelTaskWorkSessionUseCase = cancelTaskWorkSessionUseCase;
        _getTaskWorkSessionUseCase = getTaskWorkSessionUseCase;
    }

    [HttpPost("start")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TaskWorkSessionResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ClientErrorApiResponse<ErrorCodes>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ClientErrorApiResponse<ErrorCodes>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ClientErrorApiResponse<ErrorCodes>))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ClientErrorApiResponse<ErrorCodes>))]
    public async Task<ActionResult> StartSession([FromBody] StartWorkSessionRequest request, CancellationToken cancellationToken)
    {
        if (request.TaskId == Guid.Empty)
        {
            return ClientError(new ErrorApi<ErrorCodes>(ErrorCodes.IncorrectValue));
        }
        
        var userIdClaim = User.FindFirst("id")?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return ClientError(new ErrorApi<ErrorCodes>(ErrorCodes.NotAuthorized), StatusCodes.Status401Unauthorized);
        }

        var blRequest = new CreateWorkSessionBlRequest
        {
            UserId = userId,
            
            TaskId = request.TaskId,
            
            ScheduleId = request.ScheduleId,
        };
        
        var result = await _createTaskWorkSessionUseCase.CreateTaskWorkSession(blRequest, cancellationToken);
        if (result.ErrorCode is not null)
        {
            var statusCode = result.ErrorCode switch
            {
                ErrorCodes.NotAuthorized => StatusCodes.Status401Unauthorized,
                ErrorCodes.TaskNotFound => StatusCodes.Status404NotFound,
                ErrorCodes.ScheduleNotFound => StatusCodes.Status404NotFound,
                ErrorCodes.TaskShouldBeCompleted => StatusCodes.Status409Conflict,
                ErrorCodes.InvalidTaskStatusTransition => StatusCodes.Status409Conflict,
                ErrorCodes.ScheduleDoesNotMatchTask => StatusCodes.Status409Conflict,
                ErrorCodes.TaskWorkSessionAlreadyExists => StatusCodes.Status409Conflict,
                _ => StatusCodes.Status400BadRequest
            };

            return ClientError(new ErrorApi<ErrorCodes?>(result.ErrorCode), statusCode);
        }

        var response = new TaskWorkSessionResponse
        {
            SessionId = result.SessionBlResult!.SessionId,
            UserId = result.SessionBlResult.UserId,
            TaskId = result.SessionBlResult.TaskId,
            ScheduleBlockId = result.SessionBlResult.ScheduleBlockId,
            StartedAt = result.SessionBlResult.StartedAt,
            EndedAt = result.SessionBlResult.EndedAt,
            SpentMinutes = result.SessionBlResult.SpentMinutes,
            Status = result.SessionBlResult.Status,
            Source = result.SessionBlResult.Source,
            CreatedAt = result.SessionBlResult.CreatedAt,
        };
        
        return Ok(response);
    }

    [HttpPost("{sessionId}/stop")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TaskWorkSessionResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ClientErrorApiResponse<ErrorCodes>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ClientErrorApiResponse<ErrorCodes>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ClientErrorApiResponse<ErrorCodes>))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ClientErrorApiResponse<ErrorCodes>))]
    public async Task<ActionResult> StopTaskWorkSessionA([FromRoute] Guid sessionId, CancellationToken ct)
    {
        if (sessionId == Guid.Empty)
        {
            return ClientError(new ErrorApi<ErrorCodes>(ErrorCodes.IncorrectValue));
        }
        
        var userIdClaim = User.FindFirst("id")?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return ClientError(new ErrorApi<ErrorCodes>(ErrorCodes.NotAuthorized), StatusCodes.Status401Unauthorized);
        }
        
        var result = await _stopTaskWorkSessionUseCase.StopTaskWorkSession(userId, sessionId, ct);
        if (result.ErrorCode is not null)
        {
            var statusCode = result.ErrorCode switch
            {
                ErrorCodes.NotAuthorized => StatusCodes.Status401Unauthorized,
                ErrorCodes.SessionNotFound => StatusCodes.Status404NotFound,
                ErrorCodes.InvalidTaskWorkSessionStatus => StatusCodes.Status409Conflict,
                _ => StatusCodes.Status400BadRequest
            };
            return ClientError(new ErrorApi<ErrorCodes?>(result.ErrorCode), statusCode);
        }
        var response = new TaskWorkSessionResponse
        {
            SessionId = result.SessionBlResult!.SessionId,
            UserId = result.SessionBlResult.UserId,
            TaskId = result.SessionBlResult.TaskId,
            ScheduleBlockId = result.SessionBlResult.ScheduleBlockId,
            StartedAt = result.SessionBlResult.StartedAt,
            EndedAt = result.SessionBlResult.EndedAt,
            SpentMinutes = result.SessionBlResult.SpentMinutes,
            Status = result.SessionBlResult.Status,
            Source = result.SessionBlResult.Source,
            CreatedAt = result.SessionBlResult.CreatedAt,
            TaskEstimatedMinutes = result.SessionBlResult.TaskEstimatedMinutes,
            TaskActualSpentMinutes = result.SessionBlResult.TaskActualSpentMinutes
        };
        
        return Ok(response);
    }

    [HttpPost("{sessionId}/cancel")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TaskWorkSessionResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ClientErrorApiResponse<ErrorCodes>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ClientErrorApiResponse<ErrorCodes>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ClientErrorApiResponse<ErrorCodes>))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ClientErrorApiResponse<ErrorCodes>))]
    public async Task<ActionResult> CancelTaskWorkSession([FromRoute] Guid sessionId, CancellationToken ct)
    {
        if (sessionId == Guid.Empty)
        {
            return ClientError(new ErrorApi<ErrorCodes>(ErrorCodes.IncorrectValue));
        }
        
        var userIdClaim = User.FindFirst("id")?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return ClientError(new ErrorApi<ErrorCodes>(ErrorCodes.NotAuthorized), StatusCodes.Status401Unauthorized);
        }
        
        var result = await _cancelTaskWorkSessionUseCase.CancelTaskWorkSession(userId, sessionId, ct);
        if (result.ErrorCode is not null)
        {
            var statusCode = result.ErrorCode switch
            {
                ErrorCodes.NotAuthorized => StatusCodes.Status401Unauthorized,
                ErrorCodes.SessionNotFound => StatusCodes.Status404NotFound,
                ErrorCodes.InvalidTaskWorkSessionStatus => StatusCodes.Status409Conflict,
                _ => StatusCodes.Status400BadRequest
            };
            return ClientError(new ErrorApi<ErrorCodes?>(result.ErrorCode), statusCode);
        }
        var response = new TaskWorkSessionResponse
        {
            SessionId = result.SessionBlResult!.SessionId,
            UserId = result.SessionBlResult.UserId,
            TaskId = result.SessionBlResult.TaskId,
            ScheduleBlockId = result.SessionBlResult.ScheduleBlockId,
            StartedAt = result.SessionBlResult.StartedAt,
            EndedAt = result.SessionBlResult.EndedAt,
            SpentMinutes = result.SessionBlResult.SpentMinutes,
            Status = result.SessionBlResult.Status,
            Source = result.SessionBlResult.Source,
            CreatedAt = result.SessionBlResult.CreatedAt,
            TaskEstimatedMinutes = result.SessionBlResult.TaskEstimatedMinutes,
            TaskActualSpentMinutes = result.SessionBlResult.TaskActualSpentMinutes
        };
        
        return Ok(response);
    }

    [HttpGet("current")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TaskWorkSessionResponse))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ClientErrorApiResponse<ErrorCodes>))]
    public async Task<ActionResult> CurrentTaskWorkSession(CancellationToken ct)
    {
        var userIdClaim = User.FindFirst("id")?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return ClientError(new ErrorApi<ErrorCodes>(ErrorCodes.NotAuthorized), StatusCodes.Status401Unauthorized);
        }
        
        var sessionResult = await _getTaskWorkSessionUseCase.GetTaskWorkSession(userId, ct);
        if (sessionResult is null)
        {
            return Ok(null);
        }

        if (sessionResult.ErrorCode is not null)
        {
            return ClientError(new ErrorApi<ErrorCodes?>(sessionResult.ErrorCode), StatusCodes.Status401Unauthorized);
        }
        
        var response = new TaskWorkSessionResponse
        {
            SessionId = sessionResult.SessionBlResult!.SessionId,
            UserId = sessionResult.SessionBlResult.UserId,
            TaskId = sessionResult.SessionBlResult.TaskId,
            ScheduleBlockId = sessionResult.SessionBlResult.ScheduleBlockId,
            StartedAt = sessionResult.SessionBlResult.StartedAt,
            EndedAt = sessionResult.SessionBlResult.EndedAt,
            SpentMinutes = sessionResult.SessionBlResult.SpentMinutes,
            Status = sessionResult.SessionBlResult.Status,
            Source = sessionResult.SessionBlResult.Source,
            CreatedAt = sessionResult.SessionBlResult.CreatedAt,
            Title = sessionResult.SessionBlResult.Title,
            TaskEstimatedMinutes = sessionResult.SessionBlResult.TaskEstimatedMinutes,
            TaskActualSpentMinutes = sessionResult.SessionBlResult.TaskActualSpentMinutes
        };
        return Ok(response);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetAllTaskSessionByDayResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ClientErrorApiResponse<ErrorCodes>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ClientErrorApiResponse<ErrorCodes>))]
    public async Task<ActionResult> GetTaskWorkSessions([FromQuery] DateOnly? date, CancellationToken ct)
    {
        if (date is null)
        {
            return ClientError(new ErrorApi<ErrorCodes>(ErrorCodes.IncorrectValue));
        }

        var userIdClaim = User.FindFirst("id")?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return ClientError(new ErrorApi<ErrorCodes>(ErrorCodes.NotAuthorized), StatusCodes.Status401Unauthorized);
        }
        
        var sessionResult = await _getTaskWorkSessionUseCase.GetAllSessionsByDay(userId, date.Value, ct);
        if (sessionResult.ErrorCode is not null)
        {
            var statusCode = sessionResult.ErrorCode switch
            {
                ErrorCodes.NotAuthorized => StatusCodes.Status401Unauthorized,
                _ => StatusCodes.Status400BadRequest
            };

            return ClientError(new ErrorApi<ErrorCodes?>(sessionResult.ErrorCode), statusCode);
        }
        
        var sessionsList = new List<TaskWorkSessionResponse>(sessionResult.Sessions.Count);

        foreach (var session in sessionResult.Sessions)
        {
            var response = new TaskWorkSessionResponse
            {
                SessionId = session.SessionId,
                UserId = session.UserId,
                TaskId = session.TaskId,
                ScheduleBlockId = session.ScheduleBlockId,
                StartedAt = session.StartedAt,
                EndedAt = session.EndedAt,
                SpentMinutes = session.SpentMinutes,
                Status = session.Status,
                Source = session.Source,
                CreatedAt = session.CreatedAt,
                Title = session.Title,
                TaskEstimatedMinutes = session.TaskEstimatedMinutes,
                TaskActualSpentMinutes = session.TaskActualSpentMinutes
            };
            sessionsList.Add(response);
        }

        var sessionsResponse = new GetAllTaskSessionByDayResponse
        {
            TaskWorkSessionResponses = sessionsList
        };
        return Ok(sessionsResponse);
    }
}
