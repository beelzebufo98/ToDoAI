using Microsoft.EntityFrameworkCore;
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
}
