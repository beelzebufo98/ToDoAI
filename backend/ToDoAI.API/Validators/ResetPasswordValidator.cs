using FluentValidation;
using ToDoAI.API.Controllers.Auth.Models;

namespace ToDoAI.API.Validators;

public sealed class ResetPasswordValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordValidator()
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

        RuleFor(request => request.NewPassword)
            .NotEmpty()
            .WithMessage("New password is required")
            .MinimumLength(8)
            .WithMessage("New password must be at least 8 characters long")
            .MaximumLength(64)
            .WithMessage("New password must be less than 64 characters")
            .Matches("[a-z]")
            .WithMessage("New password must contain at least one lowercase letter")
            .Matches("[0-9]")
            .WithMessage("New password must contain at least one digit")
            .Matches("[^a-zA-Z0-9]")
            .WithMessage("New password must contain at least one special character");
    }
}
