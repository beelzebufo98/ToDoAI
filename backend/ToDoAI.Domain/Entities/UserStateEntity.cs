namespace ToDoAI.ToDoAI.Domain.Entities;

public sealed class UserStateEntity
{
    public Guid Id { get; init; }
    
    public int SleepMinutes { get; init; }
    
    public int EnergyLevel { get; init; }
    
    public int StressLevel { get; init; }
    
    public DateTimeOffset CreatedAt { get; init; }
    
    public Guid UserId { get; init; }

    public UserEntity User { get; init; } = default!;
}
