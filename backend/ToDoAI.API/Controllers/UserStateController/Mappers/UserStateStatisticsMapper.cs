using ToDoAI.API.Controllers.UserStateController.Models;
using ToDoAI.Application.UseCases.UserStateUseCase.Models;

namespace ToDoAI.API.Controllers.UserStateController.Mappers;

public static class UserStateStatisticsMapper
{
    public static UserStateStatisticsResponse ToUserStateStatisticsResponse(this UserStateStatisticsResult result)
    {
        return new UserStateStatisticsResponse
        {
            PeriodDays = result.PeriodDays,
            EntriesCount = result.EntriesCount,
            DaysWithEntries = result.DaysWithEntries,
            Averages = result.Averages.ToAggregateUserStateStatisticsResponse(),
            Minimums = result.Minimums.ToAggregateUserStateStatisticsResponse(),
            Maximums = result.Maximums.ToAggregateUserStateStatisticsResponse(),
            DateStatistics = result.DateStatistics
                .Select(x => x.ToDateStatisticsResponse())
                .ToList()
        };
    }

    private static AggregateUserStateStatisticsResponse ToAggregateUserStateStatisticsResponse(
        this AggregateUserStateStatistics result)
    {
        return new AggregateUserStateStatisticsResponse
        {
            SleepMinutes = result.SleepMinutes,
            EnergyLevel = result.EnergyLevel,
            StressLevel = result.StressLevel,
            MotivationLevel = result.MotivationLevel,
            ConcentrationLevel = result.ConcentrationLevel
        };
    }

    private static DateStatisticsResponse ToDateStatisticsResponse(this DateStatistics result)
    {
        return new DateStatisticsResponse
        {
            CreatedDate = result.CreatedDate,
            EntriesCount = result.EntriesCount,
            SleepMinutes = result.SleepMinutes,
            EnergyLevel = result.EnergyLevel,
            StressLevel = result.StressLevel,
            MotivationLevel = result.MotivationLevel,
            ConcentrationLevel = result.ConcentrationLevel
        };
    }
}