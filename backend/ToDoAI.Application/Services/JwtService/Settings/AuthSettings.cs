namespace ToDoAI.ToDoAI.Application.Services.JwtService.Settings;

public sealed class AuthSettings
{
    public string TokenLifetime { get; set; }
    
    public string SecretKey { get; set; }
    
}