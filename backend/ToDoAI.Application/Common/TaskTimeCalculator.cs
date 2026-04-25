using ToDoAI.Domain.Enums;

namespace ToDoAI.Application.Common;

public static class TaskTimeCalculator
{
    public static int CalculateRemainingMinutes(
        int estimatedMinutes,
        int actualSpentMinutes,
        WorkStatus workStatus)
    {
        if (workStatus == WorkStatus.Completed)
        {
            return 0;
        }

        if (actualSpentMinutes < estimatedMinutes)
        {
            return estimatedMinutes - actualSpentMinutes;
        }

        return Math.Min(Math.Max(estimatedMinutes / 3, 30), 60);
    }
}
