using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ToDoAI.Application.Abstractions.DalProviders.CreateTaskDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.GetTaskDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.RefreshTokenDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.UserDalProvider;
using ToDoAI.Infrastructure.DalProviders.CreateTaskDalProvider;
using ToDoAI.Infrastructure.DalProviders.GetTaskDalProvider;
using ToDoAI.Infrastructure.DalProviders.RefreshTokenDalProvider;
using ToDoAI.Infrastructure.DalProviders.UserDalProvider;
using ToDoAI.Infrastructure.Data;

namespace ToDoAI.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default");

        services.AddDbContextFactory<ToDoAIDbContext>(options => ConfigureDatabase(options, connectionString));

        services.AddScoped<IRefreshTokenDalProvider, RefreshTokenDalProvider>();
        services.AddScoped<IUserDalProvider, UserDalProvider>();
        services.AddScoped<ICreateTaskDalProvider, CreateTaskDalProvider>();
        services.AddScoped<IGetTaskDalProvider, GetTaskDalProvider>();

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
