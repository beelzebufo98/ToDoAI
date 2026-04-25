using Microsoft.Extensions.Options;

namespace ToDoAI.Application.Services.AiService.Settings;

public sealed class AiServiceSettingsValidator : IValidateOptions<AiServiceSettings>
{
    public ValidateOptionsResult Validate(string? name, AiServiceSettings options)
    {
        if (!options.Enabled)
        {
            return ValidateOptionsResult.Success;
        }

        if (string.IsNullOrWhiteSpace(options.BaseUrl))
        {
            return ValidateOptionsResult.Fail("AiService:BaseUrl is required when AiService is enabled.");
        }

        if (!Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out _))
        {
            return ValidateOptionsResult.Fail("AiService:BaseUrl must be an absolute URI.");
        }

        if (string.IsNullOrWhiteSpace(options.GenerateSchedulePath))
        {
            return ValidateOptionsResult.Fail("AiService:GenerateSchedulePath is required.");
        }

        if (options.TimeoutSeconds <= 0)
        {
            return ValidateOptionsResult.Fail("AiService:TimeoutSeconds must be greater than zero.");
        }

        return ValidateOptionsResult.Success;
    }
}
