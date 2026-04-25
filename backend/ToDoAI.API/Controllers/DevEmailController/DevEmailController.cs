using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using ToDoAI.API.Controllers;
using ToDoAI.API.Controllers.DevEmailController.Models;
using ToDoAI.Application.Services.EmailService;

namespace ToDoAI.API.Controllers.DevEmailController;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/dev/email")]
public sealed class DevEmailController : ToDoAiControllerBase
{
    private readonly IEmailService _emailService;
    private readonly IWebHostEnvironment _environment;

    public DevEmailController(IEmailService emailService, IWebHostEnvironment environment)
    {
        _emailService = emailService;
        _environment = environment;
    }

    [HttpPost("test")]
    [ProducesResponseType(typeof(PayloadApiResponse<SendTestEmailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> SendTestEmail([FromBody] SendTestEmailRequest request, CancellationToken cancellationToken)
    {
        if (!_environment.IsDevelopment())
        {
            return NotFound();
        }

        var normalizedType = request.Type.Trim().ToLowerInvariant();
        var code = Random.Shared.Next(100000, 999999).ToString();

        switch (normalizedType)
        {
            case "confirmation":
                await _emailService.SendEmailConfirmationAsync(request.Email, code, cancellationToken);
                break;

            case "password-reset":
                await _emailService.SendPasswordResetAsync(request.Email, code, cancellationToken);
                break;

            default:
                ModelState.AddModelError(nameof(request.Type), "Допустимые значения: confirmation, password-reset.");
                return ValidationProblem(ModelState);
        }

        return Ok(new SendTestEmailResponse
        {
            Email = request.Email,
            Type = normalizedType,
            Code = code
        });
    }
}
