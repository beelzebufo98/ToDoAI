using FluentValidation;
using ToDoAI.API.Controllers.Auth.Models;

namespace ToDoAI.API.Validators;

public sealed class ConfirmEmailValidator : AbstractValidator<ConfirmEmailRequest>
{
    public ConfirmEmailValidator()
    {
        RuleFor(request => request.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .MaximumLength(256)
            .WithMessage("Email must be less than 256 characters")
            .EmailAddress()
            .WithMessage("Email must be valid");

        RuleFor(request => request.Code)
            .NotEmpty()
            .WithMessage("Code is required")
            .Length(6)
            .WithMessage("Code must be exactly 6 characters long")
            .Matches("^[0-9]{6}$")
            .WithMessage("Code must contain only digits");
    }
}
