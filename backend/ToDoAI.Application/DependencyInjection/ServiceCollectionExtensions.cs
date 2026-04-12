using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ToDoAI.Application.Services.JwtService;
using ToDoAI.Application.Services.JwtService.Settings;
using ToDoAI.Application.UseCases.CreateTask;
using ToDoAI.Application.UseCases.CreateUser;
using ToDoAI.Application.UseCases.DeleteTask;
using ToDoAI.Application.UseCases.GetTask;
using ToDoAI.Application.UseCases.LoginUser;
using ToDoAI.Application.UseCases.RefreshToken;
using ToDoAI.Application.UseCases.TaskExecutionUseCase;
using ToDoAI.Application.UseCases.UpdateTask;
using ToDoAI.Application.UseCases.UpdateTaskStatus;
using ToDoAI.Application.UseCases.UserStateUseCase;

namespace ToDoAI.Application.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ICreateUserUseCase, CreateUserUseCase>();
        services.AddScoped<ILoginUserUseCase, LoginUserUseCase>();
        services.AddScoped<IRefreshTokenUseCase, RefreshTokenUseCase>();
        services.AddScoped<ICreateTaskUseCase, CreateTaskUseCase>();
        services.AddScoped<IGetTaskUseCase, GetTaskUseCase>();
        services.AddScoped<IUpdateTaskStatusUseCase, UpdateTaskStatusUseCase>();
        services.AddScoped<IUpdateTaskUseCase,  UpdateTaskUseCase>();
        services.AddScoped<IDeleteTaskUseCase, DeleteTaskUseCase>();
        services.AddScoped<IUserStateUseCase, UserStateUseCase>();
        services.AddScoped<ITaskExecutionUseCase, TaskExecutionUseCase>();
        services.AddScoped<IJwtService, JwtService>();

        services.AddSingleton<IValidateOptions<AuthSettings>, AuthSettingsValidator>();
        services.AddOptions<AuthSettings>()
            .BindConfiguration("AuthSettings")
            .ValidateOnStart();

        return services;
    }
}
