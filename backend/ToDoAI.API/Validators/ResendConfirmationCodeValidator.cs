using FluentValidation;
using ToDoAI.API.Controllers.Auth.Models;

namespace ToDoAI.API.Validators;

public sealed class ResendConfirmationCodeValidator : AbstractValidator<ResendConfirmationCodeRequest>
{
    public ResendConfirmationCodeValidator()
    {
        RuleFor(request => request.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .MaximumLength(256)
            .WithMessage("Email must be less than 256 characters")
            .EmailAddress()
            .WithMessage("Email must be valid");
    }
}