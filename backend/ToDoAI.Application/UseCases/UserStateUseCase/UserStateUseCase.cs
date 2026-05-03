using ToDoAI.Application.Abstractions.DalProviders.UserDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.UserStateDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.UserStateDalProvider.Models;
using ToDoAI.Application.UseCases.UserStateUseCase.Mappers;
using ToDoAI.Application.UseCases.UserStateUseCase.Models;
using ToDoAI.Domain.Enums;

namespace ToDoAI.Application.UseCases.UserStateUseCase;

public sealed class UserStateUseCase : IUserStateUseCase
{
    private static readonly TimeSpan UserLocalOffset = TimeSpan.FromHours(3);
    private readonly IUserDalProvider _userDalProvider;
    private readonly IUserStateDalProvider _userStateDalProvider;
    
    public UserStateUseCase(IUserDalProvider userDalProvider, IUserStateDalProvider userStateDalProvider)
    {
        _userDalProvider = userDalProvider;
        _userStateDalProvider = userStateDalProvider;
    }

    public async Task<UserStateStatisticsBlResult> GetUserStateStatistics(Guid userId, int days,
        CancellationToken cancellationToken)
    {
        var user = await _userDalProvider.GetUser(userId, cancellationToken);

        if (user == null)
        {
            return new UserStateStatisticsBlResult
            {
                ErrorCode = ErrorCodes.NotAuthorized
            };
        }
        
        var userStates = await _userStateDalProvider.GetUserStatesByDays(userId, days, cancellationToken);
        var orderedStates = userStates
            .OrderBy(x => x.CreatedAt)
            .ToList();

        var entriesCount = orderedStates.Count;
        var entriesByDayCount = orderedStates
            .Select(x => x.CreatedAt.ToOffset(UserLocalOffset).Date)
            .Distinct()
            .Count();

        var dailyStatistics = orderedStates
            .GroupBy(x => DateOnly.FromDateTime(x.CreatedAt.ToOffset(UserLocalOffset).DateTime.Date))
            .OrderBy(x => x.Key)
            .Select(x => new DateStatistics
            {
                CreatedDate = x.Key,
                EntriesCount = x.Count(),
                SleepMinutes = RoundAverage(x.Select(y => y.SleepMinutes)),
                EnergyLevel = RoundAverage(x.Select(y => y.EnergyLevel)),
                StressLevel = RoundAverage(x.Select(y => y.StressLevel)),
                MotivationLevel = RoundAverage(x.Select(y => y.MotivationLevel)),
                ConcentrationLevel = RoundAverage(x.Select(y => y.ConcentrationLevel))
            })
            .ToList();

        var statistics = new UserStateStatisticsResult
        {
            PeriodDays = days,
            EntriesCount = entriesCount,
            DaysWithEntries = entriesByDayCount,
            Averages = CreateAggregateStatistics(
                orderedStates,
                values => RoundAverage(values.Select(x => x.SleepMinutes)),
                values => RoundAverage(values.Select(x => x.EnergyLevel)),
                values => RoundAverage(values.Select(x => x.StressLevel)),
                values => RoundAverage(values.Select(x => x.MotivationLevel)),
                values => RoundAverage(values.Select(x => x.ConcentrationLevel))),
            Minimums = CreateAggregateStatistics(
                orderedStates,
                values => values.Min(x => x.SleepMinutes),
                values => values.Min(x => x.EnergyLevel),
                values => values.Min(x => x.StressLevel),
                values => values.Min(x => x.MotivationLevel),
                values => values.Min(x => x.ConcentrationLevel)),
            Maximums = CreateAggregateStatistics(
                orderedStates,
                values => values.Max(x => x.SleepMinutes),
                values => values.Max(x => x.EnergyLevel),
                values => values.Max(x => x.StressLevel),
                values => values.Max(x => x.MotivationLevel),
                values => values.Max(x => x.ConcentrationLevel)),
            DateStatistics = dailyStatistics
        };

        return new UserStateStatisticsBlResult
        {
            UserStateStatistics = statistics
        };
    }

    public async Task<UserStateHistoryBlResult> GetUserStateHistory(Guid userId, int limit,
        CancellationToken cancellationToken)
    {
        var user = await _userDalProvider.GetUser(userId, cancellationToken);

        if (user == null)
        {
            return new UserStateHistoryBlResult
            {
                ErrorCode = ErrorCodes.NotAuthorized
            };
        }
        
        var userStates = await _userStateDalProvider.GetUserStates(userId, limit, cancellationToken);
        var result = userStates.Select(x => x.ToUserStateResult()).ToList();
        
        return new UserStateHistoryBlResult
        {
            UserState = result
        };
    }
    public async Task<UserStateBlResult> GetLatestUserState(Guid userId, CancellationToken cancellationToken)
    {
        var user = await _userDalProvider.GetUser(userId, cancellationToken);

        if (user == null)
        {
            return new UserStateBlResult
            {
                ErrorCode = ErrorCodes.NotAuthorized
            };
        }
        
        var result = await _userStateDalProvider.GetLatestUserState(userId, cancellationToken);
        if (result == null)
        {
            return new UserStateBlResult
            {
                ErrorCode = ErrorCodes.UserStateNotFound
            };
        }
        
        return new UserStateBlResult
        {
            UserState = result.ToUserStateResult()
        };
    }

    public async Task<UserStateBlResult> CreateUserState(CreateUserStateBlRequest userStateRequest, CancellationToken cancellationToken)
    {
        var user = await _userDalProvider.GetUser(userStateRequest.UserId, cancellationToken);

        if (user == null)
        {
            return new UserStateBlResult
            {
                ErrorCode = ErrorCodes.NotAuthorized
            };
        }
        
        var stateResult = await _userStateDalProvider.GetLatestUserState(userStateRequest.UserId, cancellationToken);
        var now = DateTimeOffset.UtcNow;
        UserStateDal result = null!;
        var shouldCreateNewState =
            stateResult == null ||
            stateResult.CreatedAt.ToOffset(UserLocalOffset).Date != now.ToOffset(UserLocalOffset).Date ||
            stateResult.CreatedAt.AddHours(3) < now;

        if (shouldCreateNewState)
        {
            var dalRequest = new UserStateDalRequest
            {
                UserId = userStateRequest.UserId,
                UserStateId = Guid.NewGuid(),
                CreatedAt = now,
                SleepMinutes = userStateRequest.SleepMinutes,
                EnergyLevel = userStateRequest.EnergyLevel,
                StressLevel = userStateRequest.StressLevel,
                MotivationLevel = userStateRequest.MotivationLevel,
                ConcentrationLevel = userStateRequest.ConcentrationLevel,
            };

            result = await _userStateDalProvider.CreateUserState(dalRequest, cancellationToken);
        }
        else
        {
            var updateRequest = new UpdateUserStateDalRequest
            {
                StateId = stateResult.UserStateId,
                UserId = userStateRequest.UserId,
                SleepMinutes = userStateRequest.SleepMinutes,
                EnergyLevel = userStateRequest.EnergyLevel,
                StressLevel = userStateRequest.StressLevel,
                MotivationLevel = userStateRequest.MotivationLevel,
                ConcentrationLevel = userStateRequest.ConcentrationLevel,
            };
            result = await _userStateDalProvider.UpdateUserState(updateRequest,  cancellationToken);
        }
        
        return new UserStateBlResult
        {
            UserState = result.ToUserStateResult()
        };
    }

    private static AggregateUserStateStatistics CreateAggregateStatistics(
        IReadOnlyCollection<UserStateDal> states,
        Func<IReadOnlyCollection<UserStateDal>, int> sleepSelector,
        Func<IReadOnlyCollection<UserStateDal>, int> energySelector,
        Func<IReadOnlyCollection<UserStateDal>, int> stressSelector,
        Func<IReadOnlyCollection<UserStateDal>, int> motivationSelector,
        Func<IReadOnlyCollection<UserStateDal>, int> concentrationSelector)
    {
        if (states.Count == 0)
        {
            return new AggregateUserStateStatistics();
        }

        return new AggregateUserStateStatistics
        {
            SleepMinutes = sleepSelector(states),
            EnergyLevel = energySelector(states),
            StressLevel = stressSelector(states),
            MotivationLevel = motivationSelector(states),
            ConcentrationLevel = concentrationSelector(states)
        };
    }

    private static int RoundAverage(IEnumerable<int> values)
    {
        var materializedValues = values.ToList();
        if (materializedValues.Count == 0)
        {
            return 0;
        }

        return (int)Math.Round(materializedValues.Average(), MidpointRounding.AwayFromZero);
    }
}
