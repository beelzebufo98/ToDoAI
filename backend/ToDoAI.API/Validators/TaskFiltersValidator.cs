using FluentValidation;
using ToDoAI.API.Controllers.TaskController.Models;

namespace ToDoAI.API.Validators;

public sealed class TaskFiltersValidator : AbstractValidator<TaskFiltersRequest>
{
    public TaskFiltersValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
            .When(x => x.Page.HasValue);

        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(1)
            .LessThanOrEqualTo(100);
    }
}