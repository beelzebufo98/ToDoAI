using FluentValidation;
using ToDoAI.API.Controllers.TaskController.Models;

namespace ToDoAI.API.Validators;

public sealed class UpdateTaskValidator : AbstractValidator<UpdateTaskRequest>
{
    public UpdateTaskValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .When(x => x.Title is not null);

        RuleFor(x => x.Description)
            .NotEmpty()
            .When(x => x.Description is not null);

        RuleFor(x => x.EstimatedMinutes)
            .GreaterThan(0)
            .When(x => x.EstimatedMinutes.HasValue);

        RuleFor(x => x.ComplexityLevel)
            .GreaterThan(0)
            .LessThanOrEqualTo(10)
            .When(x => x.ComplexityLevel.HasValue);

        RuleFor(x => x.Priority)
            .GreaterThan(0)
            .LessThanOrEqualTo(10)
            .When(x => x.Priority.HasValue);
    }
}