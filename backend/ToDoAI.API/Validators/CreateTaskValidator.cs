using FluentValidation;
using ToDoAI.API.Controllers.TaskController.Models;

namespace ToDoAI.API.Validators;

public sealed class CreateTaskValidator : AbstractValidator<CreateTaskRequest>
{
    public CreateTaskValidator()
    {
        RuleFor(r => r.Title)
            .NotEmpty();

        RuleFor(r => r.Description)
            .NotEmpty();

        RuleFor(r => r.EstimatedMinutes)
            .GreaterThan(0);

        RuleFor(r => r.ComplexityLevel)
            .GreaterThan(0)
            .LessThanOrEqualTo(10);
        
        RuleFor(r => r.Priority)
            .GreaterThan(0)
            .LessThanOrEqualTo(10);
    }
}
