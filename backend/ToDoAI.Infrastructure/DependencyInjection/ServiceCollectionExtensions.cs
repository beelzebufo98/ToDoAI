using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ToDoAI.Application.Abstractions.DalProviders.CreateTaskDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.DeleteTaskDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.GetTaskDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.RefreshTokenDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.ScheduleDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.TaskExecutionDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.UpdateTaskDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.UpdateTaskStatusDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.UserDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.UserStateDalProvider;
using ToDoAI.Infrastructure.DalProviders.CreateTaskDalProvider;
using ToDoAI.Infrastructure.DalProviders.DeleteDalProvider;
using ToDoAI.Infrastructure.DalProviders.GetTaskDalProvider;
using ToDoAI.Infrastructure.DalProviders.RefreshTokenDalProvider;
using ToDoAI.Infrastructure.DalProviders.ScheduleDalProvider;
using ToDoAI.Infrastructure.DalProviders.TaskExecutionDalProvider;
using ToDoAI.Infrastructure.DalProviders.UpdateTaskDalProvider;
using ToDoAI.Infrastructure.DalProviders.UpdateTaskStatusDalProvider;
using ToDoAI.Infrastructure.DalProviders.UserDalProvider;
using ToDoAI.Infrastructure.DalProviders.UserStateDalProvider;
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
        services.AddScoped<IDeleteTaskDalProvider, DeleteTaskDalProvider>();
        services.AddScoped<IUpdateTaskDalProvider, UpdateTaskDalProvider>();
        services.AddScoped<IUpdateTaskStatusDalProvider, UpdateTaskStatusDalProvider>();
        services.AddScoped<IUserStateDalProvider, UserStateDalProvider>();
        services.AddScoped<ITaskExecutionDalProvider, TaskExecutionDalProvider>();
        services.AddScoped<IScheduleDalProvider, ScheduleDalProvider>();

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
