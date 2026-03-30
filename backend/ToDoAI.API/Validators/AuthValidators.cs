using FluentValidation;
using ToDoAI.ToDoAI.API.Controllers.Auth.Models;

namespace ToDoAI.ToDoAI.API.Validators;

public sealed class AuthValidators : AbstractValidator<RegisterUserRequest>
{
    public AuthValidators()
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
        
        RuleFor(user => user.FirstName)
            .NotEmpty()
            .WithMessage("First name is required")
            .Length(1, 100)
            .WithMessage("First name must be between 1 and 100 characters")
            .Matches("^[a-zA-Zа-яА-Я]+$")
            .WithMessage("First name must contain only letters");
        
        RuleFor(user => user.LastName)
            .MaximumLength(100)
            .WithMessage("Last name must not exceed 100 characters")
            .Matches("^[a-zA-Zа-яА-Я]*$")
            .WithMessage("Last name must contain only letters")
            .When(user => !string.IsNullOrEmpty(user.LastName));
        
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