using ToDoAI.Application.Abstractions.DalProviders.UserStateDalProvider.Models;
using ToDoAI.Domain.Entities;

namespace ToDoAI.Infrastructure.DalProviders.UserStateDalProvider.Mappers;

public static class UserStateMapper
{
    public static UserStateEntity ToUserStateEntity(this UserStateDalRequest userStateDal)
    {
        return new UserStateEntity
        {
            Id = userStateDal.UserStateId,
            SleepMinutes = userStateDal.SleepMinutes,
            EnergyLevel = userStateDal.EnergyLevel,
            StressLevel = userStateDal.StressLevel,
            MotivationLevel = userStateDal.MotivationLevel,
            ConcentrationLevel = userStateDal.ConcentrationLevel,
            CreatedAt = userStateDal.CreatedAt,
            UserId = userStateDal.UserId,
        };
    }

    public static UserStateDal ToUserStateDal(this UserStateEntity userStateEntity)
    {
        return new UserStateDal
        {
            UserStateId = userStateEntity.Id,
            SleepMinutes = userStateEntity.SleepMinutes,
            EnergyLevel = userStateEntity.EnergyLevel,
            StressLevel = userStateEntity.StressLevel,
            MotivationLevel = userStateEntity.MotivationLevel,
            ConcentrationLevel = userStateEntity.ConcentrationLevel,
            CreatedAt = userStateEntity.CreatedAt
        };
    }
}

