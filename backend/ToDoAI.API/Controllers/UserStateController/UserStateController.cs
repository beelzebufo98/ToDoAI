using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    private readonly IUserStateUseCase  _userStateUseCase;
    
    public UserStateController(IUserStateUseCase userStateUseCase)
    {
        _userStateUseCase = userStateUseCase;
    }

    [HttpPost("create")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CreateUserStateResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ClientErrorApiResponse<ErrorCodes>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ClientErrorApiResponse<ErrorCodes>))]
    public async Task<ActionResult> CreateUserState([FromBody] CreateUserStateRequest userStateRequest, CancellationToken cancellation)
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
        
        var result = await _userStateUseCase.CreateUserState(blRequest, cancellation);

        if (result.ErrorCode is not null)
        {
            return ClientError(new ErrorApi<ErrorCodes?>(result.ErrorCode));
        }

        var response = new CreateUserStateResponse
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
}