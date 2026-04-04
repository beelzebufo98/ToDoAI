using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ToDoAI.Application.Services.JwtService;
using ToDoAI.Application.Services.JwtService.Settings;
using ToDoAI.Application.UseCases.CreateTask;
using ToDoAI.Application.UseCases.CreateUser;
using ToDoAI.Application.UseCases.GetTask;
using ToDoAI.Application.UseCases.LoginUser;
using ToDoAI.Application.UseCases.RefreshToken;

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
        services.AddScoped<IJwtService, JwtService>();

        services.AddSingleton<IValidateOptions<AuthSettings>, AuthSettingsValidator>();
        services.AddOptions<AuthSettings>()
            .BindConfiguration("AuthSettings")
            .ValidateOnStart();

        return services;
    }
}
