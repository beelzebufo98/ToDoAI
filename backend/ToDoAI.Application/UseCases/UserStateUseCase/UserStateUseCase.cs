using ToDoAI.Application.Abstractions.DalProviders.UserDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.UserStateDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.UserStateDalProvider.Models;
using ToDoAI.Application.UseCases.UserStateUseCase.Mappers;
using ToDoAI.Application.UseCases.UserStateUseCase.Models;
using ToDoAI.Domain.Enums;

namespace ToDoAI.Application.UseCases.UserStateUseCase;

public sealed class UserStateUseCase : IUserStateUseCase
{
    private readonly IUserDalProvider _userDalProvider;
    private readonly IUserStateDalProvider _userStateDalProvider;
    
    public UserStateUseCase(IUserDalProvider userDalProvider, IUserStateDalProvider userStateDalProvider)
    {
        _userDalProvider = userDalProvider;
        _userStateDalProvider = userStateDalProvider;
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
        
        var dalRequest = new UserStateDalRequest
        {
            UserId = userStateRequest.UserId,
            UserStateId = Guid.NewGuid(),
            CreatedAt = DateTimeOffset.UtcNow,
            SleepMinutes = userStateRequest.SleepMinutes,
            EnergyLevel = userStateRequest.EnergyLevel,
            StressLevel = userStateRequest.StressLevel,
            MotivationLevel = userStateRequest.MotivationLevel,
            ConcentrationLevel = userStateRequest.ConcentrationLevel,
        };
        
        var result = await _userStateDalProvider.CreateUserState(dalRequest, cancellationToken);
        return new UserStateBlResult
        {
            UserState = result.ToUserStateResult()
        };
    }
}