using Microsoft.Extensions.Options;

namespace ToDoAI.Application.Services.JwtService.Settings;

public sealed class AuthSettingsValidator : IValidateOptions<AuthSettings>
{
    public ValidateOptionsResult Validate(string? name, AuthSettings options)
    {
        if (string.IsNullOrWhiteSpace(options.SecretKey))
        {
            return ValidateOptionsResult.Fail("SecretKey is required.");
        }

        if (string.IsNullOrWhiteSpace(options.AccessTokenLifetime))
        {
            return ValidateOptionsResult.Fail("AccessTokenLifetime is required.");
        }

        if (string.IsNullOrWhiteSpace(options.RefreshTokenLifetime))
        {
            return ValidateOptionsResult.Fail("RefreshTokenLifetime is required.");
        }
        
        return ValidateOptionsResult.Success;
    }
}