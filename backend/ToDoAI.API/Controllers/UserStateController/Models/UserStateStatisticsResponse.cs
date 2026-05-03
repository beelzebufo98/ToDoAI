namespace ToDoAI.API.Controllers.UserStateController.Models;

public sealed class UserStateStatisticsResponse
{
    public int PeriodDays { get; set; }
    
    public int EntriesCount { get; set; }
    
    public int DaysWithEntries { get; set; }
    
    public AggregateUserStateStatisticsResponse Averages { get; set; } = new();
    
    public AggregateUserStateStatisticsResponse Minimums { get; set; } = new();
    
    public AggregateUserStateStatisticsResponse Maximums { get; set; } = new();

    public IReadOnlyCollection<DateStatisticsResponse> DateStatistics { get; set; } = [];
}