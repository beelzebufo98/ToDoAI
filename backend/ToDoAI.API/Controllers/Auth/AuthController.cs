using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using ToDoAI.ToDoAI.API.Controllers.Auth.Models;
using ToDoAI.ToDoAI.Application.UseCases.CreateUser;
using ToDoAI.ToDoAI.Application.UseCases.CreateUser.Models;
using ToDoAI.ToDoAI.Application.UseCases.LoginUser;
using ToDoAI.ToDoAI.Application.UseCases.LoginUser.Models;
using ErrorCodes = ToDoAI.ToDoAI.Domain.Enums.ErrorCodes;

namespace ToDoAI.ToDoAI.API.Controllers.Auth;

[ApiController]
[EnableCors]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth")]
public sealed class AuthController : ToDoAiControllerBase
{
    private readonly ICreateUserUseCase _createUserUseCase;
    private readonly ILoginUserUseCase  _loginUserUseCase;
    private readonly ILogger<AuthController> _logger;
    
    public AuthController(ICreateUserUseCase createUserUseCase,
        ILoginUserUseCase loginUserUseCase,
        ILogger<AuthController> logger)
    {
        _createUserUseCase = createUserUseCase;
        _loginUserUseCase = loginUserUseCase;
        _logger = logger;
    }
    
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ClientErrorApiResponse<ErrorCodes>))]
    public async Task<ActionResult> Register([FromBody]  RegisterUserRequest request, CancellationToken cancellationToken)
    {
        var blRequest = new RegisterUserBlRequest
        {
            UserName = request.UserName,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Password = request.Password
        };
        var result = await _createUserUseCase.CreateUser(blRequest, cancellationToken);
        if (result.Error is not null)
        {
            return ClientError(new ErrorApi<ErrorCodes?>(result.Error));
        }

        return Ok();
    }

    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK,  Type = typeof(LoginUserResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ClientErrorApiResponse<ErrorCodes>))]
    public async Task<ActionResult> Login([FromBody] LoginUserRequest request, CancellationToken cancellationToken)
    {
        var userRequest = new LoginUserBlRequest
        {
            Username = request.Username,
            Password = request.Password
        };
       var result = await _loginUserUseCase.LoginUser(userRequest, cancellationToken);

       if (result.Error is not null)
       {
           return ClientError(new ErrorApi<ErrorCodes?>(result.Error));
       }

       var response = new LoginUserResponse
       {
           Token = result.Token!
       };
       return Ok(response);
    }
    
}