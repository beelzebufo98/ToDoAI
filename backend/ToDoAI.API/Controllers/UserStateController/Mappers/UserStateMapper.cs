using ToDoAI.API.Controllers.UserStateController.Models;
using ToDoAI.Application.UseCases.UserStateUseCase.Models;

namespace ToDoAI.API.Controllers.UserStateController.Mappers;

public static class UserStateMapper
{
    public static UserStateResponse ToUserStateResponse(this UserStateResult result)
    {
        return new UserStateResponse
        {
            Id = result.UserStateId,
            CreatedAt = result.CreatedAt,
            SleepMinutes = result.SleepMinutes,
            EnergyLevel = result.EnergyLevel,
            StressLevel = result.StressLevel,
            MotivationLevel = result.MotivationLevel,
            ConcentrationLevel = result.ConcentrationLevel,
        };
    }
}
