using FluentValidation;
using ToDoAI.API.Controllers.TaskController.Models;

namespace ToDoAI.API.Validators;

public sealed class TaskStatusValidator : AbstractValidator<TaskStatusRequest>
{
    public TaskStatusValidator()
    {
        RuleFor(x => x.WorkStatus)
            .IsInEnum();
    }
}