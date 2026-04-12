using FluentValidation;
using ToDoAI.API.Controllers.TaskExecutionController.Models;

namespace ToDoAI.API.Validators;

public sealed class TaskExecutionValidator : AbstractValidator<CreateTaskExecutionRequest>
{
    public TaskExecutionValidator()
    {
        RuleFor(x => x.ActualMinutes).GreaterThan(0);
        RuleFor(x => x.EnergyAfter).GreaterThan(0).LessThanOrEqualTo(10);
        RuleFor(x => x.StressAfter).GreaterThan(0).LessThanOrEqualTo(10);
    }
}