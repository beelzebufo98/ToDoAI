using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ToDoAI.Application.Services.AiService.Settings;
using ToDoAI.Application.Services.EmailService;
using ToDoAI.Application.Services.EmailService.Settings;
using ToDoAI.Application.Services.JwtService;
using ToDoAI.Application.Services.JwtService.Settings;
using ToDoAI.Application.UseCases.ConfirmEmail;
using ToDoAI.Application.UseCases.CreateTask;
using ToDoAI.Application.UseCases.CreateUser;
using ToDoAI.Application.UseCases.DeleteTask;
using ToDoAI.Application.UseCases.ForgotPassword;
using ToDoAI.Application.UseCases.GetTask;
using ToDoAI.Application.UseCases.LoginUser;
using ToDoAI.Application.UseCases.RefreshToken;
using ToDoAI.Application.UseCases.ResendConfirmationCode;
using ToDoAI.Application.UseCases.ResetPassword;
using ToDoAI.Application.UseCases.GenerateSchedule;
using ToDoAI.Application.UseCases.GetSchedule;
using ToDoAI.Application.UseCases.TaskExecutionUseCase;
using ToDoAI.Application.UseCases.TaskWorkSession;
using ToDoAI.Application.UseCases.UpdateTask;
using ToDoAI.Application.UseCases.UpdateTaskStatus;
using ToDoAI.Application.UseCases.UserStateUseCase;

namespace ToDoAI.Application.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ICreateUserUseCase, CreateUserUseCase>();
        services.AddScoped<IConfirmEmailUseCase, ConfirmEmailUseCase>();
        services.AddScoped<IForgotPasswordUseCase, ForgotPasswordUseCase>();
        services.AddScoped<IResendConfirmationCodeUseCase, ResendConfirmationCodeUseCase>();
        services.AddScoped<IResetPasswordUseCase, ResetPasswordUseCase>();
        services.AddScoped<ILoginUserUseCase, LoginUserUseCase>();
        services.AddScoped<IRefreshTokenUseCase, RefreshTokenUseCase>();
        services.AddScoped<ICreateTaskUseCase, CreateTaskUseCase>();
        services.AddScoped<IGetTaskUseCase, GetTaskUseCase>();
        services.AddScoped<IUpdateTaskStatusUseCase, UpdateTaskStatusUseCase>();
        services.AddScoped<IUpdateTaskUseCase,  UpdateTaskUseCase>();
        services.AddScoped<IDeleteTaskUseCase, DeleteTaskUseCase>();
        services.AddScoped<IUserStateUseCase, UserStateUseCase>();
        services.AddScoped<ITaskExecutionUseCase, TaskExecutionUseCase>();
        services.AddScoped<IGenerateScheduleUseCase, GenerateScheduleUseCase>();
        services.AddScoped<IGetScheduleUseCase, GetScheduleUseCase>();
        services.AddScoped<ICreateTaskWorkSessionUseCase, CreateTaskWorkSessionUseCase>();
        services.AddScoped<IStopTaskWorkSessionUseCase, StopTaskWorkSessionUseCase>();
        services.AddScoped<ICancelTaskWorkSessionUseCase, CancelTaskWorkSessionUseCase>();
        services.AddScoped<IGetTaskWorkSessionUseCase, GetTaskWorkSessionUseCase>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IEmailService, EmailService>();

        services.AddSingleton<IValidateOptions<AiServiceSettings>, AiServiceSettingsValidator>();
        services.AddOptions<AiServiceSettings>()
            .BindConfiguration("AiService")
            .ValidateOnStart();

        services.AddSingleton<IValidateOptions<AuthSettings>, AuthSettingsValidator>();
        services.AddOptions<AuthSettings>()
            .BindConfiguration("AuthSettings")
            .ValidateOnStart();

        services.AddSingleton<IValidateOptions<EmailSettings>, EmailSettingsValidator>();
        services.AddOptions<EmailSettings>()
            .BindConfiguration("EmailSettings")
            .ValidateOnStart();

        return services;
    }
}
