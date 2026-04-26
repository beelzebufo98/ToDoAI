using System.ComponentModel.DataAnnotations;

namespace ToDoAI.API.Controllers.DevEmailController.Models;

public sealed class SendTestEmailRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required]
    public string Type { get; init; } = string.Empty;
}
