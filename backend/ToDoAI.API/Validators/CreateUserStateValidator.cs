using FluentValidation;
using ToDoAI.API.Controllers.UserStateController.Models;

namespace ToDoAI.API.Validators;

public sealed class CreateUserStateValidator :  AbstractValidator<CreateUserStateRequest>
{
    public CreateUserStateValidator()
    {
        RuleFor(x => x.SleepMinutes).GreaterThanOrEqualTo(0).LessThanOrEqualTo(1440);
        
        RuleFor(x => x.ConcentrationLevel).GreaterThan(0).LessThanOrEqualTo(10);
        
        RuleFor(x => x.EnergyLevel).GreaterThan(0).LessThanOrEqualTo(10);
        
        RuleFor(x => x.MotivationLevel).GreaterThan(0).LessThanOrEqualTo(10);
        
        RuleFor(x => x.StressLevel).GreaterThan(0).LessThanOrEqualTo(10);
    }
}