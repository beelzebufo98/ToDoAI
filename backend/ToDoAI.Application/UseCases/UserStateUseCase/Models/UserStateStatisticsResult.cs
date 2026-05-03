namespace ToDoAI.Application.UseCases.UserStateUseCase.Models;

public sealed record UserStateStatisticsResult
{
    public int PeriodDays { get; set; }
    
    public int EntriesCount { get; set; }
    
    public int DaysWithEntries { get; set; }
    
    public AggregateUserStateStatistics Averages { get; set; } = new();
    
    public AggregateUserStateStatistics Minimums { get; set; } = new();
    
    public AggregateUserStateStatistics Maximums { get; set; } = new();

    public IReadOnlyCollection<DateStatistics> DateStatistics { get; set; } = [];
}