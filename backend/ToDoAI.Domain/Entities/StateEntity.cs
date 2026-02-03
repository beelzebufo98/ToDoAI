namespace ToDoAI.ToDoAI.Domain.Entities;

public sealed class StateEntity
{
    public int SleepMinutes { get; set; }
    
    public int EnergyLevel { get; set; }
    
    public int StressLevel { get; set; }
    
    public DateTimeOffset UpdatedAt { get; set; }
    
    public Guid UserId { get; set; }
}