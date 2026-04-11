namespace ToDoAI.API.Controllers.UserStateController.Models;

public sealed class CreateUserStateRequest
{
    public int SleepMinutes { get; init; }
    
    public int EnergyLevel { get; init; }
    
    public int StressLevel { get; init; }
    
    public int MotivationLevel { get; init; }
    
    public int ConcentrationLevel { get; init; }
}