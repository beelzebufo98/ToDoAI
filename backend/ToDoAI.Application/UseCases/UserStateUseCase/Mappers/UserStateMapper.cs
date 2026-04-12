using ToDoAI.Application.Abstractions.DalProviders.UserStateDalProvider.Models;
using ToDoAI.Application.UseCases.UserStateUseCase.Models;

namespace ToDoAI.Application.UseCases.UserStateUseCase.Mappers;

public static class UserStateMapper
{
    public static UserStateResult ToUserStateResult(this UserStateDal result)
    {
        return new UserStateResult
        {
            UserStateId = result.UserStateId,
            CreatedAt = result.CreatedAt,
            SleepMinutes = result.SleepMinutes,
            EnergyLevel = result.EnergyLevel,
            StressLevel = result.StressLevel,
            MotivationLevel = result.MotivationLevel,
            ConcentrationLevel = result.ConcentrationLevel,
        };
    }
}