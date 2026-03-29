using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;
using ToDoAI.ToDoAI.API.Controllers.Auth.Models;
using ToDoAI.ToDoAI.API.Validators;
using ToDoAI.ToDoAI.Application.DependencyInjection;
using ToDoAI.ToDoAI.Infrastructure.DependencyInjection;
using ToDoAI.ToDoAI.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication(builder.Configuration);

builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddScoped<IValidator<RegisterUserRequest>, AuthValidators>();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddAuthentication("Bearer").AddJwtBearer();  
builder.Services.AddAuthorization();   

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ToDoAI Service API", Version = "v1" });
    c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

using (var scope = app.Services.CreateScope())
{
    var dbContextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<ToDoAIDbContext>>();
    await using var db = await dbContextFactory.CreateDbContextAsync();
    await db.Database.MigrateAsync();
}

app.UseAuthentication();
app.UseAuthorization();
app.UseHttpsRedirection();

app.MapControllers();
Console.WriteLine("App is starting...");
app.Run();
