using Microsoft.Extensions.Options;

namespace ToDoAI.ToDoAI.Application.Services.JwtService.Settings;

public sealed class AuthSettingsValidator : IValidateOptions<AuthSettings>
{
    public ValidateOptionsResult Validate(string? name, AuthSettings options)
    {
        if (string.IsNullOrWhiteSpace(options.SecretKey))
        {
            return ValidateOptionsResult.Fail("SecretKey is required.");
        }

        if (string.IsNullOrWhiteSpace(options.TokenLifetime))
        {
            return ValidateOptionsResult.Fail("TokenLifetime is required.");
        }
        return ValidateOptionsResult.Success;
    }
}