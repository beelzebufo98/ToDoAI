using Microsoft.Extensions.Options;
using ToDoAI.ToDoAI.Application.Services.JwtService;
using ToDoAI.ToDoAI.Application.Services.JwtService.Settings;
using ToDoAI.ToDoAI.Application.UseCases.CreateTask;
using ToDoAI.ToDoAI.Application.UseCases.CreateUser;
using ToDoAI.ToDoAI.Application.UseCases.LoginUser;
using ToDoAI.ToDoAI.Application.UseCases.RefreshTokenUseCase;

namespace ToDoAI.ToDoAI.Application.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ICreateUserUseCase, CreateUserUseCase>();
        services.AddScoped<ILoginUserUseCase, LoginUserUseCase>();
        services.AddScoped<IRefreshTokenUseCase, RefreshTokenUseCase>();
        services.AddScoped<ICreateTaskUseCase, CreateTaskUseCase>();
        services.AddScoped<IJwtService, JwtService>();

        services.AddSingleton<IValidateOptions<AuthSettings>, AuthSettingsValidator>();
        services.AddOptions<AuthSettings>()
            .BindConfiguration("AuthSettings")
            .ValidateOnStart();

        return services;
    }
}
