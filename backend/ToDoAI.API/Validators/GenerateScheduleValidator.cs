using FluentValidation;
using ToDoAI.API.Controllers.ScheduleController.Models;

namespace ToDoAI.API.Validators;

public sealed class GenerateScheduleValidator : AbstractValidator<GenerateScheduleRequest>
{
    public GenerateScheduleValidator()
    {
        RuleFor(x => x.ScheduleDate)
            .Must(x => x != default)
            .WithMessage("Schedule date is required.");

        RuleFor(x => x.StartAt)
            .Must(x => x != default)
            .WithMessage("Start time is required.");

        RuleFor(x => x)
            .Must(x => DateOnly.FromDateTime(x.StartAt.DateTime) == x.ScheduleDate)
            .WithMessage("Start time must belong to schedule date.");

        RuleFor(x => x.TaskIds)
            .NotEmpty();

        RuleFor(x => x.TaskIds)
            .Must(x => x.Distinct().Count() == x.Count)
            .WithMessage("Task ids must be unique.");
    }
}
