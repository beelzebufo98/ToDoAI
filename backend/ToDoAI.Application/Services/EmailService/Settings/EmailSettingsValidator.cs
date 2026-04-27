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

        if (string.IsNullOrWhiteSpace(options.FromAddress))
        {
            return ValidateOptionsResult.Fail("EmailSettings.FromAddress обязателен, когда отправка email включена.");
        }

        if (string.IsNullOrWhiteSpace(options.FromName))
        {
            return ValidateOptionsResult.Fail("EmailSettings.FromName обязателен, когда отправка email включена.");
        }

        return options.Provider switch
        {
            EmailProvider.Smtp => ValidateSmtp(options),
            EmailProvider.Resend => ValidateResend(options),
            _ => ValidateOptionsResult.Fail("EmailSettings.Provider имеет неподдерживаемое значение.")
        };
    }

    private static ValidateOptionsResult ValidateSmtp(EmailSettings options)
    {
        if (string.IsNullOrWhiteSpace(options.Host))
        {
            return ValidateOptionsResult.Fail("EmailSettings.Host обязателен, когда выбран SMTP.");
        }

        if (options.Port <= 0 || options.Port > 65535)
        {
            return ValidateOptionsResult.Fail("EmailSettings.Port должен быть в диапазоне 1..65535.");
        }

        var hasUserName = !string.IsNullOrWhiteSpace(options.UserName);
        var hasPassword = !string.IsNullOrWhiteSpace(options.Password);

        if (hasUserName != hasPassword)
        {
            return ValidateOptionsResult.Fail("EmailSettings.UserName и EmailSettings.Password должны быть заполнены вместе или оба пустыми.");
        }

        return ValidateOptionsResult.Success;
    }

    private static ValidateOptionsResult ValidateResend(EmailSettings options)
    {
        if (string.IsNullOrWhiteSpace(options.ApiKey))
        {
            return ValidateOptionsResult.Fail("EmailSettings.ApiKey обязателен, когда выбран Resend.");
        }

        return ValidateOptionsResult.Success;
    }
}
