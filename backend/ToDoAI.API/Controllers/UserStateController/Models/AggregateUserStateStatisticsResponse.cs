namespace ToDoAI.API.Controllers.UserStateController.Models;

public sealed class AggregateUserStateStatisticsResponse
{
    public int SleepMinutes { get; set; }
        
    public int EnergyLevel { get; set; }
        
    public int StressLevel { get; set; }
        
    public int MotivationLevel { get; set; }
        
    public int ConcentrationLevel { get; set; }
}