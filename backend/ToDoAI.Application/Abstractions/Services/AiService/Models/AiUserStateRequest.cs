namespace ToDoAI.Application.Abstractions.Services.AiService.Models;

public sealed record AiUserStateRequest
{
    public int SleepMinutes { get; init; }

    public int EnergyLevel { get; init; }

    public int StressLevel { get; init; }

    public int MotivationLevel { get; init; }

    public int ConcentrationLevel { get; init; }
}