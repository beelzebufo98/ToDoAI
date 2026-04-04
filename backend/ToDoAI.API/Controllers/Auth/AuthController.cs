using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;
using ToDoAI.ToDoAI.API.Controllers.Auth.Models;
using ToDoAI.ToDoAI.Application.Services.JwtService;
using ToDoAI.ToDoAI.Application.Services.JwtService.Settings;
using ToDoAI.ToDoAI.Application.UseCases.CreateUser;
using ToDoAI.ToDoAI.Application.UseCases.CreateUser.Models;
using ToDoAI.ToDoAI.Application.UseCases.LoginUser;
using ToDoAI.ToDoAI.Application.UseCases.LoginUser.Models;
using ToDoAI.ToDoAI.Application.UseCases.RefreshToken;
using ToDoAI.ToDoAI.Application.UseCases.RefreshToken.Models;
using ToDoAI.ToDoAI.Infrastructure.DalProviders.UserDalProvider;
using ErrorCodes = ToDoAI.ToDoAI.Domain.Enums.ErrorCodes;

namespace ToDoAI.ToDoAI.API.Controllers.Auth;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth")]
public sealed class AuthController : ToDoAiControllerBase
{
    private readonly ICreateUserUseCase _createUserUseCase;
    private readonly ILoginUserUseCase  _loginUserUseCase;
    private readonly IRefreshTokenUseCase _refreshTokenUseCase;
    private readonly IUserDalProvider _userDalProvider;
    private readonly IJwtService _jwtService;
    private readonly IOptions<AuthSettings> _authSettings;
    
    public AuthController(
        ICreateUserUseCase createUserUseCase,
        ILoginUserUseCase loginUserUseCase,
        IRefreshTokenUseCase refreshTokenUseCase,
        IUserDalProvider userDalProvider,
        IJwtService jwtService,
        IOptions<AuthSettings> authSettings)
    {
        _createUserUseCase = createUserUseCase;
        _loginUserUseCase = loginUserUseCase;
        _refreshTokenUseCase = refreshTokenUseCase;
        _userDalProvider = userDalProvider;
        _jwtService = jwtService;
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

    [HttpPost("refresh")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ClientErrorApiResponse<ErrorCodes>))]
    public async Task<ActionResult> Refresh(CancellationToken cancellationToken)
    {
        if (!HttpContext.Request.Cookies.TryGetValue("refreshToken", out var refreshToken))
        {
            return ClientError(new ErrorApi<ErrorCodes>(ErrorCodes.NotAuthorized), StatusCodes.Status401Unauthorized);
        }

        Guid userId;
        try
        {
            userId = _jwtService.GetUserIdFromToken(refreshToken);
        }
        catch (SecurityTokenException)
        {
            DeleteAuthCookies();
            return ClientError(new ErrorApi<ErrorCodes>(ErrorCodes.NotAuthorized), StatusCodes.Status401Unauthorized);
        }

        var refreshTokenHash = _jwtService.HashRefreshToken(refreshToken);
        var refreshSession = await _refreshTokenUseCase.GetRefreshToken(refreshTokenHash, cancellationToken);
        if (refreshSession is null || refreshSession.ExpiresAt <= DateTimeOffset.UtcNow)
        {
            DeleteAuthCookies();
            return ClientError(new ErrorApi<ErrorCodes>(ErrorCodes.NotAuthorized), StatusCodes.Status401Unauthorized);
        }

        var account = await _userDalProvider.GetUser(userId, cancellationToken);
        if (account is null)
        {
            DeleteAuthCookies();
            return ClientError(new ErrorApi<ErrorCodes>(ErrorCodes.NotAuthorized), StatusCodes.Status401Unauthorized);
        }

        await _refreshTokenUseCase.DeleteRefreshToken(refreshTokenHash, cancellationToken);

        var accessToken = _jwtService.GenerateAccessToken(account);
        var newRefreshToken = _jwtService.GenerateRefreshToken(account);
        var newRefreshTokenHash = _jwtService.HashRefreshToken(newRefreshToken);

        var refreshTokenExpiresAt = GetRefreshTokenExpiration();
        await _refreshTokenUseCase.CreateRefreshToken(new RefreshTokenBlRequest
        {
            UserId = account.UserId,
            RefreshTokenHash = newRefreshTokenHash,
            ExpiresAt = refreshTokenExpiresAt,
            CreatedAt = DateTimeOffset.UtcNow
        }, cancellationToken);

        SetAuthCookies(accessToken, newRefreshToken, refreshTokenExpiresAt);
        return Ok();
    }

    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> Logout(CancellationToken cancellationToken)
    {
        if (HttpContext.Request.Cookies.TryGetValue("refreshToken", out var refreshToken))
        {
            var refreshTokenHash = _jwtService.HashRefreshToken(refreshToken);
            await _refreshTokenUseCase.DeleteRefreshToken(refreshTokenHash, cancellationToken);
        }

        DeleteAuthCookies();
        return NoContent();
    }

    [NonAction]
    private void SetAuthCookies(string accessToken, string refreshToken, DateTimeOffset refreshTokenExpiresAt)
    {
        var accessCookieExpiration = DateTimeOffset.UtcNow.AddMinutes(15);
        if (TimeSpan.TryParse(_authSettings.Value.AccessTokenLifetime, out var accessTokenLifetime))
        {
            accessCookieExpiration = DateTimeOffset.UtcNow.Add(accessTokenLifetime);
        }

        HttpContext.Response.Cookies.Append("accessToken", accessToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = HttpContext.Request.IsHttps,
            SameSite = SameSiteMode.Lax,
            Expires = accessCookieExpiration
        });

        HttpContext.Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = HttpContext.Request.IsHttps,
            SameSite = SameSiteMode.Lax,
            Expires = refreshTokenExpiresAt
        });
    }

    [NonAction]
    private DateTimeOffset GetRefreshTokenExpiration()
    {
        var refreshCookieExpiration = DateTimeOffset.UtcNow.AddDays(7);
        if (TimeSpan.TryParse(_authSettings.Value.RefreshTokenLifetime, out var refreshTokenLifetime))
        {
            refreshCookieExpiration = DateTimeOffset.UtcNow.Add(refreshTokenLifetime);
        }

        return refreshCookieExpiration;
    }

    [NonAction]
    private void DeleteAuthCookies()
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = HttpContext.Request.IsHttps,
            SameSite = SameSiteMode.Lax
        };

        HttpContext.Response.Cookies.Delete("accessToken", cookieOptions);
        HttpContext.Response.Cookies.Delete("refreshToken", cookieOptions);
    }
}
