using Microsoft.Extensions.Options;

namespace ToDoAI.Application.Services.EmailService.Settings;

public sealed class EmailSettingsValidator : IValidateOptions<EmailSettings>
{
    public ValidateOptionsResult Validate(string? name, EmailSettings options)
    {
        if (!options.Enabled)
        {
            return ValidateOptionsResult.Success;
        }

        if (string.IsNullOrEmpty(options.Host))
        {
            return ValidateOptionsResult.Fail("EmailSettings.Host обязателен, когда отправка email включена.");
        }

        if (options.Port <= 0 || options.Port > 65535)
        {
            return ValidateOptionsResult.Fail("EmailSettings.Port должен быть в диапазоне 1..65535.");
        }

        if (string.IsNullOrEmpty(options.FromAddress))
        {
            return ValidateOptionsResult.Fail("EmailSettings.FromAddress обязателен, когда отправка email включена.");
        }

        if (string.IsNullOrEmpty(options.FromName))
        {
            return ValidateOptionsResult.Fail("EmailSettings.FromName обязателен, когда отправка email включена.");
        }

        var hasUserName = !string.IsNullOrWhiteSpace(options.UserName);
        var hasPassword = !string.IsNullOrWhiteSpace(options.Password);

        if (hasUserName != hasPassword)
        {
            return ValidateOptionsResult.Fail("EmailSettings.UserName и EmailSettings.Password должны быть заполнены вместе или оба пустыми.");
        }
        
        return ValidateOptionsResult.Success;
    }
}
