namespace ToDoAI.Domain.Entities;

public sealed class UserStateEntity
{
    public Guid Id { get; init; }
    
    public int SleepMinutes { get; set; }
    
    public int EnergyLevel { get; set; }
    
    public int StressLevel { get; set; }
    
    public int MotivationLevel  { get; set; }
    
    public int ConcentrationLevel  { get; set; }
    
    public DateTimeOffset CreatedAt { get; init; }
    
    public Guid UserId { get; init; }

    public UserEntity User { get; init; } = default!;
}
