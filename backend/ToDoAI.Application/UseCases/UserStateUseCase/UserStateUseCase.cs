using ToDoAI.Application.Abstractions.DalProviders.UserDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.UserStateDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.UserStateDalProvider.Models;
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
            UserState = new UserStateResult
            {
                UserStateId = result.UserStateId,
                CreatedAt = result.CreatedAt,
                SleepMinutes = result.SleepMinutes,
                EnergyLevel = result.EnergyLevel,
                StressLevel = result.StressLevel,
                MotivationLevel = result.MotivationLevel,
                ConcentrationLevel = result.ConcentrationLevel
            }
        };
    }
}