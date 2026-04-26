using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Antiforgery;
using ToDoAI.Infrastructure.Data;

namespace ToDoAI.API.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication UseSwaggerDocs(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "ToDoAI API v1.0");
            c.SwaggerEndpoint("/swagger/internal/swagger.json", "ToDoAI Internal API v1.0");
        });
        app.UseSwaggerUI(c =>
        {
            c.RoutePrefix = "internal/swagger";
            c.DocumentTitle = "ToDoAI Internal API";
            c.SwaggerEndpoint("/swagger/internal/swagger.json", "ToDoAI Internal API v1.0");
        });

        return app;
    }

    public static async Task<WebApplication> UseDatabaseMigrations(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var dbContextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<ToDoAIDbContext>>();
        await using var db = await dbContextFactory.CreateDbContextAsync();
        await db.Database.MigrateAsync();

        return app;
    }

    public static WebApplication UseCsrfProtection(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            return app;
        }

        app.Use(async (context, next) =>
        {
            if (!RequiresCsrfValidation(context))
            {
                await next();
                return;
            }

            var antiforgery = context.RequestServices.GetRequiredService<IAntiforgery>();

            try
            {
                await antiforgery.ValidateRequestAsync(context);
                await next();
            }
            catch (AntiforgeryValidationException)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(new
                {
                    error = new
                    {
                        code = "invalid_csrf_token"
                    }
                });
            }
        });

        return app;
    }

    private static bool RequiresCsrfValidation(HttpContext context)
    {
        if (HttpMethods.IsGet(context.Request.Method) ||
            HttpMethods.IsHead(context.Request.Method) ||
            HttpMethods.IsOptions(context.Request.Method) ||
            HttpMethods.IsTrace(context.Request.Method))
        {
            return false;
        }

        var path = context.Request.Path;
        if (path.StartsWithSegments("/swagger"))
        {
            return false;
        }

        if (path.StartsWithSegments("/api", out var remaining))
        {
            var tail = remaining.Value ?? string.Empty;
            if (tail.Contains("/auth/login", StringComparison.OrdinalIgnoreCase) ||
                tail.Contains("/auth/register", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        return context.Request.Cookies.ContainsKey("accessToken") ||
               context.Request.Cookies.ContainsKey("refreshToken");
    }
}
