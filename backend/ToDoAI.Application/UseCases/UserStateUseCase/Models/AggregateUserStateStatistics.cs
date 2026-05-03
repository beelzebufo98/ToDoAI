namespace ToDoAI.Application.UseCases.UserStateUseCase.Models;

public sealed record AggregateUserStateStatistics
{
    public int SleepMinutes { get; set; }
        
    public int EnergyLevel { get; set; }
        
    public int StressLevel { get; set; }
        
    public int MotivationLevel { get; set; }
        
    public int ConcentrationLevel { get; set; }
}