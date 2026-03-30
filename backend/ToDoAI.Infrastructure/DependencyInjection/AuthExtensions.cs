using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using ToDoAI.ToDoAI.Application.Services.JwtService.Settings;

namespace ToDoAI.ToDoAI.Infrastructure.DependencyInjection;

public static class AuthExtensions
{
    public static IServiceCollection AddJwtService(this IServiceCollection services, IConfiguration configuration)
    {
        var authSettings = configuration.GetSection(nameof(AuthSettings)).Get<AuthSettings>();
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(o =>
            {
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authSettings!.SecretKey))
                };
            });
        return services;
    }
}