using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ToDoAI.ToDoAI.API.Controllers.Auth.Models;
using ToDoAI.ToDoAI.Application.Services.JwtService.Settings;
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
    private readonly IOptions<AuthSettings> _authSettings;
    
    public AuthController(
        ICreateUserUseCase createUserUseCase,
        ILoginUserUseCase loginUserUseCase,
        IOptions<AuthSettings> authSettings)
    {
        _createUserUseCase = createUserUseCase;
        _loginUserUseCase = loginUserUseCase;
        _authSettings = authSettings;
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

        return Created();
    }

    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ClientErrorApiResponse<ErrorCodes>))]
    public async Task<ActionResult> Login([FromBody] LoginUserRequest request, CancellationToken cancellationToken)
    {
        var userRequest = new LoginUserBlRequest
        {
            UserName = request.UserName,
            Password = request.Password
        };
       var result = await _loginUserUseCase.LoginUser(userRequest, cancellationToken);

       if (result.Error is not null)
       {
           return ClientError(new ErrorApi<ErrorCodes?>(result.Error));
       }

       var cookieExpiration = DateTimeOffset.UtcNow.AddMinutes(15);
       if (TimeSpan.TryParse(_authSettings.Value.AccessTokenLifetime, out var tokenLifetime))
       {
           cookieExpiration = DateTimeOffset.UtcNow.Add(tokenLifetime);
       }

       var refreshCookieExpiration = DateTimeOffset.UtcNow.AddDays(7);
       if (TimeSpan.TryParse(_authSettings.Value.RefreshTokenLifetime, out var refreshTokenLifetime))
       {
           refreshCookieExpiration = DateTimeOffset.UtcNow.Add(refreshTokenLifetime);
       }

       HttpContext.Response.Cookies.Append("accessToken", result.AccessToken!, new CookieOptions()
       {
           HttpOnly = true,
           Secure = HttpContext.Request.IsHttps,
           SameSite = SameSiteMode.Lax,
           Expires = cookieExpiration
       });

       HttpContext.Response.Cookies.Append("refreshToken", result.RefreshToken!, new CookieOptions()
       {
           HttpOnly = true,
           Secure = HttpContext.Request.IsHttps,
           SameSite = SameSiteMode.Lax,
           Expires = refreshCookieExpiration
        });
       
       return Ok();
    }
}