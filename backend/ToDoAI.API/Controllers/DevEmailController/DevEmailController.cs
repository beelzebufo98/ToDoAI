using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ToDoAI.API.Controllers;
using ToDoAI.API.Controllers.DevEmailController.Models;
using ToDoAI.API.Settings;
using ToDoAI.Application.Services.EmailService;

namespace ToDoAI.API.Controllers.DevEmailController;

[ApiController]
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "internal")]
[Route("api/v{version:apiVersion}/dev/email")]
public sealed class DevEmailController : ToDoAiControllerBase
{
    private readonly IEmailService _emailService;
    private readonly IWebHostEnvironment _environment;
    private readonly DevEmailSettings _settings;

    public DevEmailController(
        IEmailService emailService,
        IWebHostEnvironment environment,
        IOptions<DevEmailSettings> settings)
    {
        _emailService = emailService;
        _environment = environment;
        _settings = settings.Value;
    }

    [HttpPost("test")]
    [ProducesResponseType(typeof(PayloadApiResponse<SendTestEmailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> SendTestEmail(
        [FromBody] SendTestEmailRequest request,
        [FromHeader(Name = "X-Dev-Email-Token")] string? devToken,
        CancellationToken cancellationToken)
    {
        if (!_environment.IsDevelopment() || !_settings.Enabled || string.IsNullOrWhiteSpace(_settings.Token))
        {
            return NotFound();
        }

        if (!string.Equals(devToken, _settings.Token, StringComparison.Ordinal))
        {
            return Unauthorized();
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
