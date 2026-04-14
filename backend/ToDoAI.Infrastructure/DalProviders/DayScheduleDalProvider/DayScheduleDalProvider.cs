using Microsoft.EntityFrameworkCore;
using ToDoAI.Application.Abstractions.DalProviders.DayScheduleDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.DayScheduleDalProvider.Models;
using ToDoAI.Domain.Entities;
using ToDoAI.Infrastructure.Data;

namespace ToDoAI.Infrastructure.DalProviders.DayScheduleDalProvider;

public sealed class DayScheduleDalProvider : IDayScheduleDalProvider
{
    private readonly IDbContextFactory<ToDoAIDbContext> _dbContextFactory;

    public DayScheduleDalProvider(IDbContextFactory<ToDoAIDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<DayScheduleDalResult> CreateDaySchedule(ScheduleDalRequest request, CancellationToken cancellationToken)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        var existingSchedules = await dbContext.DaySchedules
            .Where(x => x.UserId == request.UserId && x.Date == request.ScheduleDate)
            .ToListAsync(cancellationToken);

        foreach (var existingSchedule in existingSchedules.Where(x => x.IsActiveVersion))
        {
            existingSchedule.IsActiveVersion = false;
        }

        var nextVersion = existingSchedules.Count == 0
            ? 1
            : existingSchedules.Max(x => x.Version) + 1;

        var dayScheduleEntity = new DayScheduleEntity
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            Date = request.ScheduleDate,
            Version = nextVersion,
            IsActiveVersion = true,
            CreatedAt = DateTimeOffset.UtcNow,
            Blocks = []
        };

        var cursor = request.StartAt;
        foreach (var task in request.TaskList)
        {
            var slotStart = cursor;
            var slotEnd = slotStart.AddMinutes(task.EstimatedMinutes);

            dayScheduleEntity.Blocks.Add(new ScheduleEntity
            {
                Id = Guid.NewGuid(),
                DayScheduleId = dayScheduleEntity.Id,
                TaskId = task.Id,
                Start = slotStart,
                End = slotEnd,
            });

            cursor = slotEnd;
        }

        await dbContext.DaySchedules.AddAsync(dayScheduleEntity, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new DayScheduleDalResult
        {
            DayScheduleId = dayScheduleEntity.Id,
            UserId = dayScheduleEntity.UserId,
            ScheduleDate = dayScheduleEntity.Date,
            Version = dayScheduleEntity.Version,
            IsActiveVersion = dayScheduleEntity.IsActiveVersion,
            CreateDate = dayScheduleEntity.CreatedAt,
            Blocks = dayScheduleEntity.Blocks
                .Select(x => new ScheduleDalResult
                {
                    ScheduleId = x.Id,
                    TaskId = x.TaskId,
                    StartAt = x.Start,
                    EndAt = x.End,
                })
                .ToList()
        };
    }
}
