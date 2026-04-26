using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ToDoAI.API.Controllers.TaskExecutionController.Models;
using ToDoAI.Application.UseCases.TaskExecutionUseCase;
using ToDoAI.Application.UseCases.TaskExecutionUseCase.Models;
using ToDoAI.Domain.Enums;

namespace ToDoAI.API.Controllers.TaskExecutionController;

[ApiController]
[Authorize]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/task-execution/")]
public sealed class TaskExecutionController : ToDoAiControllerBase
{
    private readonly ITaskExecutionUseCase  _taskExecutionUseCase;
    
    public TaskExecutionController(ITaskExecutionUseCase taskExecutionUseCase)
    {
        _taskExecutionUseCase = taskExecutionUseCase;
    }

    [HttpPost("{taskId}/create")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TaskExecutionResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ClientErrorApiResponse<ErrorCodes>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ClientErrorApiResponse<ErrorCodes>))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ClientErrorApiResponse<ErrorCodes>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ClientErrorApiResponse<ErrorCodes>))]
    public async Task<ActionResult> CreateTaskExecution([FromRoute] string taskId,[FromBody] CreateTaskExecutionRequest request, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst("id")?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return ClientError(new ErrorApi<ErrorCodes>(ErrorCodes.NotAuthorized), StatusCodes.Status401Unauthorized);
        }
        
        if (!Guid.TryParse(taskId, out var taskIdReq))
        {
            return ClientError(new ErrorApi<ErrorCodes>(ErrorCodes.TaskNotFound));
        }

        Guid? scheduleIdReq = null;

        if (request.ScheduleId is not null)
        {
            if (!Guid.TryParse(request.ScheduleId, out var parsed))
            {
                return ClientError(new ErrorApi<ErrorCodes>(ErrorCodes.IncorrectValue));
            }

            scheduleIdReq = parsed;
        }

        var blRequest = new TaskExecutionBlRequest
        {
            TaskId = taskIdReq,
            UserId = userId,
            ScheduleId = scheduleIdReq,
            ActualMinutes = request.ActualMinutes,
            StressAfter = request.StressAfter,
            EnergyAfter = request.EnergyAfter,
        };
        
        var result = await _taskExecutionUseCase.CreateTaskExecution(blRequest, cancellationToken);

        if (result.ErrorCode is not null)
        {
            var statusCode = result.ErrorCode switch
            {
                ErrorCodes.NotAuthorized => StatusCodes.Status401Unauthorized,
                ErrorCodes.TaskNotFound => StatusCodes.Status404NotFound,
                ErrorCodes.ScheduleNotFound => StatusCodes.Status404NotFound,
                ErrorCodes.TaskExecutionAlreadyExists => StatusCodes.Status409Conflict,
                ErrorCodes.TaskShouldBeCompleted => StatusCodes.Status409Conflict,
                ErrorCodes.ScheduleDoesNotMatchTask => StatusCodes.Status409Conflict,
                _ => StatusCodes.Status400BadRequest
            };

            return ClientError(new ErrorApi<ErrorCodes?>(result.ErrorCode), statusCode);
        }

        var response = new TaskExecutionResponse
        {
            TaskExecutionId = result.TaskExecutionResult.TaskExecutionId,
            TaskId = result.TaskExecutionResult.TaskId,
            ScheduleId = result.TaskExecutionResult.ScheduleId,
            ActualMinutes = result.TaskExecutionResult.ActualMinutes,
            StressAfter = result.TaskExecutionResult.StressAfter,
            EnergyAfter = result.TaskExecutionResult.EnergyAfter,
            CreatedAt = result.TaskExecutionResult.CreatedAt,
            MotivationMessage = result.TaskExecutionResult.MotivationMessage,
        };
        return Ok(response);
    }
}