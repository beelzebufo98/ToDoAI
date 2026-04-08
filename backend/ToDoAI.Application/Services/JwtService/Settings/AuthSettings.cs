namespace ToDoAI.Application.Services.JwtService.Settings;

public sealed class AuthSettings
{
    public string AccessTokenLifetime { get; init; }
    
    public string RefreshTokenLifetime { get; init; }
    
    public string SecretKey { get; init; }
    
}