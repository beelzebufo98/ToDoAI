using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ToDoAI.API.Controllers.UserController.Models;
using ToDoAI.Application.UseCases.UserUseCase;
using ToDoAI.Domain.Enums;

namespace ToDoAI.API.Controllers.UserController;

[ApiController]
[Authorize]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/user")]
public sealed class UserController : ToDoAiControllerBase
{
    private readonly IUserUseCase   _userUseCase;
    
    public UserController(IUserUseCase userUseCase)
    {
        _userUseCase = userUseCase;
    }

    [HttpGet("me")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ClientErrorApiResponse<ErrorCodes>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ClientErrorApiResponse<ErrorCodes>))]
    public async Task<ActionResult> GetMe(CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst("id")?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return ClientError(new ErrorApi<ErrorCodes>(ErrorCodes.NotAuthorized), StatusCodes.Status401Unauthorized);
        }
        
        var result = await _userUseCase.GetUser(userId, cancellationToken);
        if (result.Error is not null)
        {
            var statusCode = result.Error switch
            {
                ErrorCodes.NotAuthorized => StatusCodes.Status401Unauthorized,
                _ => StatusCodes.Status400BadRequest
            };

            return ClientError(new ErrorApi<ErrorCodes?>(result.Error), statusCode);
        }

        var response = new UserResponse
        {
            UserId = result.UserResult.UserId,
            UserName = result.UserResult.UserName,
            Email = result.UserResult.Email,
            FirstName = result.UserResult.FirstName,
            LastName = result.UserResult.LastName,
        };
        
        return Ok(response);
    }
    
}
