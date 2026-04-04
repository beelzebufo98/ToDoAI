using Microsoft.EntityFrameworkCore;
using ToDoAI.ToDoAI.Infrastructure.DalProviders.CreateTaskDalProvider;
using ToDoAI.ToDoAI.Infrastructure.DalProviders.RefreshTokenDalProvider;
using ToDoAI.ToDoAI.Infrastructure.DalProviders.UserDalProvider;
using ToDoAI.ToDoAI.Infrastructure.Data;

namespace ToDoAI.ToDoAI.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default");

        services.AddDbContextFactory<ToDoAIDbContext>(options => ConfigureDatabase(options, connectionString));

        services.AddScoped<IRefreshTokenDalProvider, RefreshTokenDalProvider>();
        services.AddScoped<IUserDalProvider, UserDalProvider>();
        services.AddScoped<ICreateTaskDalProvider, CreateTaskDalProvider>();

        return services;
    }

    private static void ConfigureDatabase(DbContextOptionsBuilder options, string? connectionString)
    {
        options.UseNpgsql(connectionString, npgsql =>
            {
                npgsql.MigrationsAssembly(typeof(ToDoAIDbContext).Assembly.FullName);
                npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "ToDoAIService");
            })
            .EnableSensitiveDataLogging()
            .EnableDetailedErrors();
    }
}
