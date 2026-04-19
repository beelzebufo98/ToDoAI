using ToDoAI.Application.Abstractions.DalProviders.DayScheduleDalProvider.Models;
using ToDoAI.Application.UseCases.GenerateSchedule.Models;

namespace ToDoAI.Application.UseCases.GenerateSchedule.Mappers;

public static class DayScheduleMapper
{
    public static DayScheduleBlResult ToDayScheduleBl(this DayScheduleDalResult daySchedule)
    {
        return new DayScheduleBlResult
        {
            DayScheduleId = daySchedule.DayScheduleId,
            UserId = daySchedule.UserId,
            ScheduleDate = daySchedule.ScheduleDate,
            Version = daySchedule.Version,
            IsActiveVersion = daySchedule.IsActiveVersion,
            CreateDate = daySchedule.CreateDate,
            Blocks = daySchedule.Blocks.Select(ToScheduleBlResult).ToList()
        };
    }
    
    private static ScheduleBlResult ToScheduleBlResult(ScheduleDalResult scheduleDal)
    {
        return new ScheduleBlResult
        {
            ScheduleId = scheduleDal.ScheduleId,
            TaskId = scheduleDal.TaskId,
            StartAt = scheduleDal.StartAt,
            EndAt = scheduleDal.EndAt,
            TaskTitle = scheduleDal.TaskTitle,
            TaskDescription = scheduleDal.TaskDescription,
        };
    }
    
}