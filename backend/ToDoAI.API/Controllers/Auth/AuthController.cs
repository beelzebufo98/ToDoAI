using Microsoft.AspNetCore.Mvc;
using ToDoAI.ToDoAI.API.Controllers.Auth.Models;
using ToDoAI.ToDoAI.Application.Services.AccountService;
using ToDoAI.ToDoAI.Application.Services.AccountService.Models;

namespace ToDoAI.ToDoAI.API.Controllers.Auth;

[ApiController]
[Route("[controller]")]
public sealed class AuthController : ControllerBase
{
    private readonly IAccountService _accountService;
    private readonly ILogger<AuthController> _logger;
    
    public AuthController(IAccountService accountService, ILogger<AuthController> logger)
    {
        _accountService = accountService;
        _logger = logger;
    }
    
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody]  RegisterUserRequest request)
    {
        var blRequest = new RegisterUserBlRequest
        {
            UserName = request.UserName,
            FirstName = request.FirstName,
            LastName = request.LastName,
        };

        return Ok();
    }
}