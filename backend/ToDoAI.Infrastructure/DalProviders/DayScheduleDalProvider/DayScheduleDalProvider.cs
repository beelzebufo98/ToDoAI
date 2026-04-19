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

        var responseOffset = request.StartAt.Offset;
        var responseOffsetMinutes = (int)responseOffset.TotalMinutes;

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
            TimeZoneOffsetMinutes = responseOffsetMinutes,
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
                Start = slotStart.ToOffset(TimeSpan.Zero),
                End = slotEnd.ToOffset(TimeSpan.Zero),
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
                    StartAt = x.Start.ToOffset(responseOffset),
                    EndAt = x.End.ToOffset(responseOffset),
                })
                .ToList()
        };
    }

    public async Task<DayScheduleDalResult?> GetDaySchedule(Guid userId, DateOnly scheduleDate,
        CancellationToken cancellationToken)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        
        var dayScheduleEntity = await dbContext.DaySchedules.AsNoTracking().Include(dayScheduleEntity => dayScheduleEntity.Blocks).ThenInclude(x => x.Task).FirstOrDefaultAsync(x => x.UserId == userId && x.Date == scheduleDate && x.IsActiveVersion == true, cancellationToken);

        if (dayScheduleEntity == null)
        {
            return null;
        }

        var responseOffset = TimeSpan.FromMinutes(dayScheduleEntity.TimeZoneOffsetMinutes);
        
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
                    StartAt = x.Start.ToOffset(responseOffset),
                    EndAt = x.End.ToOffset(responseOffset),
                    TaskTitle = x.Task.Title,
                    TaskDescription = x.Task.Description
                }).OrderBy(x => x.StartAt)
                .ToList()
        };
    }
}
