using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ToDoAI.API.Controllers.ScheduleController.Models;
using ToDoAI.Application.UseCases.GenerateSchedule;
using ToDoAI.Application.UseCases.GenerateSchedule.Models;
using ToDoAI.Domain.Enums;

namespace ToDoAI.API.Controllers.ScheduleController;

[ApiController]
[Authorize]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/schedule/")]
public sealed class ScheduleController : ToDoAiControllerBase
{
    private readonly IGenerateScheduleUseCase _generateScheduleUseCase;

    public ScheduleController(IGenerateScheduleUseCase generateScheduleUseCase)
    {
        _generateScheduleUseCase = generateScheduleUseCase;
    }

    [HttpPost("generate")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GenerateScheduleResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ClientErrorApiResponse<ErrorCodes>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ClientErrorApiResponse<ErrorCodes>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ClientErrorApiResponse<ErrorCodes>))]
    public async Task<ActionResult> GenerateSchedule([FromBody] GenerateScheduleRequest  request, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst("id")?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return ClientError(new ErrorApi<ErrorCodes>(ErrorCodes.NotAuthorized), StatusCodes.Status401Unauthorized);
        }

        var blRequest = new GenerateScheduleBlRequest
        {
            UserId = userId,
            ScheduleDate = request.ScheduleDate,
            StartAt = request.StartAt,
            TaskIds = request.TaskIds
        };

        var result = await _generateScheduleUseCase.GenerateSchedule(blRequest, cancellationToken);
        if (result.ErrorCode is not null)
        {
            var statusCode = result.ErrorCode switch
            {
                ErrorCodes.NotAuthorized => StatusCodes.Status401Unauthorized,
                ErrorCodes.TaskNotFound => StatusCodes.Status404NotFound,
                ErrorCodes.UserStateNotFound => StatusCodes.Status404NotFound,
                _ => StatusCodes.Status400BadRequest
            };

            return ClientError(new ErrorApi<ErrorCodes?>(result.ErrorCode), statusCode);
        }

        var response = new GenerateScheduleResponse
        {
            DayScheduleId = result.DaySchedule.DayScheduleId,
            ScheduleDate = result.DaySchedule.ScheduleDate,
            Version = result.DaySchedule.Version,
            IsActiveVersion = result.DaySchedule.IsActiveVersion,
            CreateDate = result.DaySchedule.CreateDate,
            Blocks = result.DaySchedule.Blocks.Select(x => new ScheduleBlockResponse
            {
                ScheduleId = x.ScheduleId,
                TaskId = x.TaskId,
                StartAt = x.StartAt,
                EndAt = x.EndAt
            }).ToList()
        };

        return Ok(response);
    }
}
