using FluentValidation;
using ToDoAI.API.Controllers.Auth.Models;

namespace ToDoAI.API.Validators;

public sealed class ForgotPasswordValidator : AbstractValidator<ForgotPasswordRequest>
{
    public ForgotPasswordValidator()
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
