namespace ToDoAI.ToDoAI.Application.Services.JwtService.Settings;

public sealed class AuthSettings
{
    public TimeSpan TokenLifetime { get; set; }
    
    public string SecretKey { get; set; }
    
}