namespace ToDoAI.Application.UseCases.UserStateUseCase.Models;

public sealed record UserStateResult
{
    public Guid UserStateId { get; init; }
    
    public DateTimeOffset CreatedAt { get; init; }
    
    public int SleepMinutes { get; init; }
    
    public int EnergyLevel { get; init; }
    
    public int StressLevel { get; init; }
    
    public int MotivationLevel { get; init; }
    
    public int ConcentrationLevel { get; init; }
}