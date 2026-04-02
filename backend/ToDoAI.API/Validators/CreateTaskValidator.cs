using FluentValidation;
using ToDoAI.ToDoAI.API.Controllers.TaskController.Models;

namespace ToDoAI.ToDoAI.API.Validators;

public sealed class CreateTaskValidator : AbstractValidator<CreateTaskRequest>
{
    public CreateTaskValidator()
    {
        RuleFor(r => r.ComplexityLevel)
            .GreaterThan(0)
            .LessThanOrEqualTo(10);
        
        RuleFor(r => r.Priority)
            .GreaterThan(0)
            .LessThanOrEqualTo(10);
    }
}