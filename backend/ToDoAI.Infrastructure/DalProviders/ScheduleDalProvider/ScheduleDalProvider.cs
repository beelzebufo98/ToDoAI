using Microsoft.EntityFrameworkCore;
using ToDoAI.Application.Abstractions.DalProviders.ScheduleDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.ScheduleDalProvider.Models;
using ToDoAI.Infrastructure.Data;

namespace ToDoAI.Infrastructure.DalProviders.ScheduleDalProvider;

public sealed class ScheduleDalProvider : IScheduleDalProvider
{
    private readonly IDbContextFactory<ToDoAIDbContext> _dbContextFactory;

    public ScheduleDalProvider(IDbContextFactory<ToDoAIDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }
    
    public async Task<ScheduleDal?> GetSchedule(Guid scheduleId, Guid userId, CancellationToken cancellationToken)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var scheduleEntity = await dbContext.Schedules
            .AsNoTracking()
            .Include(s => s.DaySchedule)
            .FirstOrDefaultAsync(s => s.Id == scheduleId && s.DaySchedule.UserId == userId, cancellationToken);

        if (scheduleEntity is null)
        {
            return null;
        }

        return new ScheduleDal
        {
            Id = scheduleEntity.Id,
            TaskId = scheduleEntity.TaskId,
            DayScheduleId = scheduleEntity.DayScheduleId,
            Start = scheduleEntity.Start,
            End = scheduleEntity.End,
        };
    }
}