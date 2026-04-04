namespace ToDoAI.API.DependencyInjection;

public static class CorsServiceCollectionExtensions
{
    public static IServiceCollection AddCorsPolicy(this IServiceCollection services, IConfiguration configuration)
    {
        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ??
                             ["http://localhost:3000", "http://localhost:5173"];

        if (allowedOrigins.Length == 0)
        {
            allowedOrigins = ["http://localhost:3000", "http://localhost:5173"];
        }

        services.AddCors(options =>
        {
            options.AddPolicy("Frontend", policy =>
            {
                policy.WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        return services;
    }
}
