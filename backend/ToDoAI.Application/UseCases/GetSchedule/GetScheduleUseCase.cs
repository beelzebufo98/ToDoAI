using ToDoAI.Application.Abstractions.DalProviders.DayScheduleDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.UserDalProvider;
using ToDoAI.Application.UseCases.GenerateSchedule.Mappers;
using ToDoAI.Application.UseCases.GenerateSchedule.Models;
using ToDoAI.Application.UseCases.GetSchedule.Models;
using ToDoAI.Domain.Enums;

namespace ToDoAI.Application.UseCases.GetSchedule;

public sealed class GetScheduleUseCase : IGetScheduleUseCase
{
    private readonly IDayScheduleDalProvider  _scheduleDalProvider;
    private readonly IUserDalProvider  _userDalProvider;

    public GetScheduleUseCase(IDayScheduleDalProvider scheduleDalProvider, IUserDalProvider userDalProvider)
    {
        _scheduleDalProvider = scheduleDalProvider;
        _userDalProvider = userDalProvider;
    }

    public async Task<GetScheduleBlResult> GetSchedule(ScheduleBlRequest request,
        CancellationToken cancellationToken)
    {
        var user = await _userDalProvider.GetUser(request.UserId, cancellationToken);

        if (user == null)
        {
            return new GetScheduleBlResult
            {
                ErrorCode = ErrorCodes.NotAuthorized
            };
        }
        
        var daySchedule = await _scheduleDalProvider.GetDaySchedule(request.UserId, request.ScheduleDate, cancellationToken);
        if (daySchedule is null)
        {
            return new GetScheduleBlResult
            {
                ErrorCode = ErrorCodes.DayScheduleNotFound
            };
        }
        
        DayScheduleBlResult dayScheduleBlResult = daySchedule.ToDayScheduleBl();
        return new GetScheduleBlResult
        {
            DaySchedule = dayScheduleBlResult
        };
    }
}
