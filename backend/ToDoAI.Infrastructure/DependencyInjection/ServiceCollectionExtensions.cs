using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ToDoAI.Application.Abstractions.Services.AiService;
using ToDoAI.Application.Abstractions.DalProviders.CreateTaskDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.DeleteTaskDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.DayScheduleDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.GetTaskDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.RefreshTokenDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.ScheduleDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.TaskExecutionDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.TaskWorkSessionDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.UpdateTaskDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.UpdateTaskStatusDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.UserDalProvider;
using ToDoAI.Application.Abstractions.DalProviders.UserStateDalProvider;
using ToDoAI.Infrastructure.DalProviders.CreateTaskDalProvider;
using ToDoAI.Infrastructure.DalProviders.DeleteDalProvider;
using ToDoAI.Infrastructure.DalProviders.DayScheduleDalProvider;
using ToDoAI.Infrastructure.DalProviders.GetTaskDalProvider;
using ToDoAI.Infrastructure.DalProviders.RefreshTokenDalProvider;
using ToDoAI.Infrastructure.DalProviders.ScheduleDalProvider;
using ToDoAI.Infrastructure.DalProviders.TaskExecutionDalProvider;
using ToDoAI.Infrastructure.DalProviders.TaskWorkSessionDalProvider;
using ToDoAI.Infrastructure.DalProviders.UpdateTaskDalProvider;
using ToDoAI.Infrastructure.DalProviders.UpdateTaskStatusDalProvider;
using ToDoAI.Infrastructure.DalProviders.UserDalProvider;
using ToDoAI.Infrastructure.DalProviders.UserStateDalProvider;
using ToDoAI.Infrastructure.Data;
using ToDoAI.Infrastructure.Clients.AiService;
using ToDoAI.Application.Services.AiService.Settings;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;

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
        services.AddScoped<ITaskWorkSessionDalProvider, TaskWorkSessionDalProvider>();
        services.AddScoped<IDayScheduleDalProvider, DayScheduleDalProvider>();
        services.AddHttpClient<IAiScheduleClient, AiServiceClient>((serviceProvider, client) =>
            {
                var settings = serviceProvider.GetRequiredService<IOptions<AiServiceSettings>>().Value;
                if (!string.IsNullOrWhiteSpace(settings.BaseUrl))
                {
                    client.BaseAddress = new Uri(settings.BaseUrl);
                }

                client.Timeout = TimeSpan.FromSeconds(settings.TimeoutSeconds);
            }
        )
            .AddPolicyHandler((serviceProvider, _) => CreateAiServiceRetryPolicy(
                serviceProvider.GetRequiredService<ILogger<AiServiceClient>>()));

        return services;
    }

    private static IAsyncPolicy<HttpResponseMessage> CreateAiServiceRetryPolicy(ILogger logger)
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(
                retryCount: 2,
                sleepDurationProvider: attempt => TimeSpan.FromMilliseconds(200 * Math.Pow(2, attempt - 1)),
                onRetry: (outcome, delay, attempt, _) =>
                {
                    var statusCode = outcome.Result is null ? "network_error" : ((int)outcome.Result.StatusCode).ToString();
                    logger.LogWarning(
                        "Retrying AI service request. Attempt {Attempt}/2 after {DelayMs} ms. Status: {StatusCode}.",
                        attempt,
                        delay.TotalMilliseconds,
                        statusCode);
                });
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
