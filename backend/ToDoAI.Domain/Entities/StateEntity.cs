namespace ToDoAI.ToDoAI.Domain.Entities;

public sealed class StateEntity
{
    public Guid Id { get; set; }
    
    public int SleepMinutes { get; set; }
    
    public int EnergyLevel { get; set; }
    
    public int StressLevel { get; set; }
    
    public DateTimeOffset UpdatedAt { get; set; }
    
    public Guid UserId { get; set; }

    public UserEntity User { get; set; } = default!;
}
