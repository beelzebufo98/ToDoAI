using FluentValidation;
using ToDoAI.ToDoAI.API.Controllers.Auth.Models;

namespace ToDoAI.ToDoAI.API.Validators;

public sealed class LoginValidator : AbstractValidator<LoginUserRequest>
{
    public LoginValidator()
    {
        RuleFor(user => user.UserName)
            .NotEmpty()
            .WithMessage("Username is required")
            .MinimumLength(6)
            .WithMessage("Username must be at least 6 characters long")
            .MaximumLength(100)
            .WithMessage("Username must be less than 100 characters")
            .Matches(@"^[a-zA-Z0-9_]*$")
            .WithMessage("Username can contain only letters, numbers and underscore");
    
        RuleFor(user => user.Password)
            .NotEmpty()
            .WithMessage("Password is required")
            .MinimumLength(8)
            .WithMessage("Password must be at least 8 characters long")
            .MaximumLength(64)
            .WithMessage("Password must be less than 64 characters")
            .Matches("[a-z]")
            .WithMessage("Password must contain at least one lowercase letter")
            .Matches("[0-9]")
            .WithMessage("Password must contain at least one digit")
            .Matches("[^a-zA-Z0-9]")
            .WithMessage("Password must contain at least one special character");
    }
}