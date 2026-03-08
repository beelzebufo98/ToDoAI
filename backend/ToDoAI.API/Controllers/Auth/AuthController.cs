using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using ToDoAI.ToDoAI.API.Controllers.Auth.Models;
using ToDoAI.ToDoAI.Application.UseCases.CreateUser;
using ToDoAI.ToDoAI.Application.UseCases.CreateUser.Models;
using ErrorCodes = ToDoAI.ToDoAI.Domain.ErrorCodes;

namespace ToDoAI.ToDoAI.API.Controllers.Auth;

[ApiController]
[EnableCors]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth")]
public sealed class AuthController : ToDoAiControllerBase
{
    private readonly ICreateUserUseCase _createUserUseCase;
    private readonly ILogger<AuthController> _logger;
    
    public AuthController(ICreateUserUseCase createUserUseCase, ILogger<AuthController> logger)
    {
        _createUserUseCase = createUserUseCase;
        _logger = logger;
    }
    
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ClientErrorApiResponse<ErrorCodes>))]
    public async Task<IActionResult> Register([FromBody]  RegisterUserRequest request)
    {
        var blRequest = new RegisterUserBlRequest
        {
            UserName = request.UserName,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Password = request.Password
        };

        return Ok();
    }
}