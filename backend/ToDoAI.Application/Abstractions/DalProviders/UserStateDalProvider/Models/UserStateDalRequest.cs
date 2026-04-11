namespace ToDoAI.Application.Abstractions.DalProviders.UserStateDalProvider.Models;

public sealed record UserStateDalRequest
{
    public Guid UserId { get; init; }
    
    public Guid UserStateId { get; init; }
    
    public DateTimeOffset CreatedAt { get; init; }
    
    public int SleepMinutes { get; init; }
    
    public int EnergyLevel { get; init; }
    
    public int StressLevel { get; init; }
    
    public int MotivationLevel { get; init; }
    
    public int ConcentrationLevel { get; init; }
}