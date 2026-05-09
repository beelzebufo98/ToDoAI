using FluentValidation;
using ToDoAI.API.Controllers.TaskController.Models;

namespace ToDoAI.API.Validators;

public sealed class AssistTaskValidator : AbstractValidator<TaskAssistRequest>
{
    public AssistTaskValidator()
    {
        RuleFor(r => r.Title)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .Must(title => title.Trim().Length >= 6)
            .WithMessage("Title must be at least 6 characters long.")
            .MaximumLength(120);

        RuleFor(r => r.Description)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .Must(description => description.Trim().Length >= 20)
            .WithMessage("Description must be at least 20 characters long.")
            .MaximumLength(2000);
        
        RuleFor(r => r.DeadlineAt)
            .Must(x => x != default)
            .WithMessage("Deadline is required.");
    }
}
