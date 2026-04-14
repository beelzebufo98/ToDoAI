using System.Text.Json;
using System.Text.Json.Serialization;
using Asp.Versioning;
using FluentValidation;
using Microsoft.AspNetCore.Antiforgery;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;
using ToDoAI.API.Controllers.Auth.Models;
using ToDoAI.API.Controllers.ScheduleController.Models;
using ToDoAI.API.Controllers.TaskController.Models;
using ToDoAI.API.Controllers.TaskExecutionController.Models;
using ToDoAI.API.Controllers.UserStateController.Models;
using ToDoAI.API.Validators;
using ToDoAI.ToDoAI.API.Controllers.TaskController.Models;
using static System.Text.Json.JsonNamingPolicy;

namespace ToDoAI.API.DependencyInjection;

public static class ApiServiceCollectionExtensions
{
    public static IServiceCollection AddApi(this IServiceCollection services)
    {
        services.AddDatabaseDeveloperPageExceptionFilter();
        services.AddAntiforgery(options =>
        {
            options.HeaderName = "X-CSRF-TOKEN";
            options.Cookie.Name = "XSRF-COOKIE";
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        });
        services.AddScoped<IValidator<RegisterUserRequest>, AuthValidator>();
        services.AddScoped<IValidator<LoginUserRequest>, LoginValidator>();
        services.AddScoped<IValidator<CreateTaskRequest>, CreateTaskValidator>();
        services.AddScoped<IValidator<TaskFiltersRequest>, TaskFiltersValidator>();
        services.AddScoped<IValidator<UpdateTaskRequest>, UpdateTaskValidator>();
        services.AddScoped<IValidator<TaskStatusRequest>, TaskStatusValidator>();
        services.AddScoped<IValidator<CreateUserStateRequest>, CreateUserStateValidator>();
        services.AddScoped<IValidator<CreateTaskExecutionRequest>, TaskExecutionValidator>();
        services.AddScoped<IValidator<GenerateScheduleRequest>, GenerateScheduleValidator>();
        services.AddFluentValidationAutoValidation();
        services.AddAuthorization();

        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(
                    new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower)
                );
                options.JsonSerializerOptions.PropertyNamingPolicy = CamelCase;
                options.JsonSerializerOptions.WriteIndented = true;
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            });

        services
            .AddApiVersioning(options =>
            {
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.ReportApiVersions = true;
            })
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

        return services;
    }
}
