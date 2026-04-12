using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ToDoAI.API.Controllers.UserStateController.Mappers;
using ToDoAI.API.Controllers.UserStateController.Models;
using ToDoAI.Application.UseCases.UserStateUseCase;
using ToDoAI.Application.UseCases.UserStateUseCase.Models;
using ToDoAI.Domain.Enums;

namespace ToDoAI.API.Controllers.UserStateController;

[ApiController]
[Authorize]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/user-state/")]
public sealed class UserStateController : ToDoAiControllerBase
{
    private const int DefaultDaysCount = 7;
    
    private readonly IUserStateUseCase  _userStateUseCase;
    
    public UserStateController(IUserStateUseCase userStateUseCase)
    {
        _userStateUseCase = userStateUseCase;
    }

    [HttpPost("create")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserStateResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ClientErrorApiResponse<ErrorCodes>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ClientErrorApiResponse<ErrorCodes>))]
    public async Task<ActionResult> CreateUserState([FromBody] CreateUserStateRequest userStateRequest, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst("id")?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return ClientError(new ErrorApi<ErrorCodes>(ErrorCodes.NotAuthorized), StatusCodes.Status401Unauthorized);
        }

        var blRequest = new CreateUserStateBlRequest
        {
            UserId = userId,
            SleepMinutes = userStateRequest.SleepMinutes,
            EnergyLevel = userStateRequest.EnergyLevel,
            StressLevel = userStateRequest.StressLevel,
            MotivationLevel = userStateRequest.MotivationLevel,
            ConcentrationLevel = userStateRequest.ConcentrationLevel
        };
        
        var result = await _userStateUseCase.CreateUserState(blRequest, cancellationToken);

        if (result.ErrorCode is not null)
        {
            return ClientError(new ErrorApi<ErrorCodes?>(result.ErrorCode));
        }

        var response = new UserStateResponse
        {
            Id = result.UserState.UserStateId,
            CreatedAt = result.UserState.CreatedAt,
            SleepMinutes = result.UserState.SleepMinutes,
            EnergyLevel = result.UserState.EnergyLevel,
            StressLevel = result.UserState.StressLevel,
            MotivationLevel = result.UserState.MotivationLevel,
            ConcentrationLevel = result.UserState.ConcentrationLevel
        };
        return Ok(response);
    }

    [HttpGet("latest")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserStateResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ClientErrorApiResponse<ErrorCodes>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ClientErrorApiResponse<ErrorCodes>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ClientErrorApiResponse<ErrorCodes>))]
    public async Task<ActionResult> GetLatestUserState(CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst("id")?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return ClientError(new ErrorApi<ErrorCodes>(ErrorCodes.NotAuthorized), StatusCodes.Status401Unauthorized);
        }
        
        var result = await _userStateUseCase.GetLatestUserState(userId, cancellationToken);

        if (result.ErrorCode is not null)
        {
            switch (result.ErrorCode)
            {
                case ErrorCodes.NotAuthorized:
                    return ClientError(new ErrorApi<ErrorCodes?>(result.ErrorCode), StatusCodes.Status401Unauthorized);
                case ErrorCodes.UserStateNotFound:
                    return ClientError(new ErrorApi<ErrorCodes?>(result.ErrorCode), StatusCodes.Status404NotFound);
                default:
                    return ClientError(new ErrorApi<ErrorCodes?>(result.ErrorCode));
            }
        }

        var response = result.UserState.ToUserStateResponse();
        return Ok(response);
    }
    
    [HttpGet("history")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserStateHistoryResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ClientErrorApiResponse<ErrorCodes>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ClientErrorApiResponse<ErrorCodes>))]
    public async Task<ActionResult> GetUserStateHistory([FromQuery] int? limit, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst("id")?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return ClientError(new ErrorApi<ErrorCodes>(ErrorCodes.NotAuthorized), StatusCodes.Status401Unauthorized);
        }
        
        if (limit.HasValue && (limit.Value <= 0 || limit.Value > 30))
        {
            return ClientError(new ErrorApi<ErrorCodes>(ErrorCodes.IncorrectValue));
        }
        
        var result = await _userStateUseCase.GetUserStateHistory(userId, limit ?? DefaultDaysCount, cancellationToken);
        if (result.ErrorCode is not null)
        {
            return ClientError(new ErrorApi<ErrorCodes?>(result.ErrorCode)); 
        }

        var response = result.UserState.Select(x => x.ToUserStateResponse()).OrderByDescending(y => y.CreatedAt).ToList();
        return Ok(new UserStateHistoryResponse
        {
            History = response
        });
    }
    
    
}