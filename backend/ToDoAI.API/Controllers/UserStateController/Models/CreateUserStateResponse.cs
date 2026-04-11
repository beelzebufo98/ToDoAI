namespace ToDoAI.API.Controllers.UserStateController.Models;

public sealed class CreateUserStateResponse
{
    public Guid Id { get; init; }
    
    public DateTimeOffset CreatedAt { get; init; }
    
    public int SleepMinutes { get; init; }
    
    public int EnergyLevel { get; init; }
    
    public int StressLevel { get; init; }
    
    public int MotivationLevel { get; init; }
    
    public int ConcentrationLevel { get; init; }
}