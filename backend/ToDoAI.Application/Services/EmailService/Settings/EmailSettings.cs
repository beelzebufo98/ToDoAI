namespace ToDoAI.Application.Services.EmailService.Settings;

public enum EmailSocketSecurityMode
{
    None = 0,
    StartTls = 1,
    SslOnConnect = 2
}

public sealed class EmailSettings
{
    public bool Enabled { get; init; } = false;

    public string Host { get; init; } = string.Empty;
    
    public int Port { get; init; } = 587;
    
    public EmailSocketSecurityMode SocketSecurityMode { get; init; } = EmailSocketSecurityMode.StartTls;
    
    public string UserName { get; init; } = string.Empty;
    
    public string Password { get; init; } = string.Empty;
    
    public string FromAddress { get; init; } = string.Empty;
    
    public string FromName { get; init; } = string.Empty;
}
