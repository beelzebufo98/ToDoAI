using ToDoAI.API.DependencyInjection;
using ToDoAI.API.Extensions;
using ToDoAI.Application.DependencyInjection;
using ToDoAI.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication(builder.Configuration);
builder.Services.AddJwtService();
builder.Services.AddApi();
builder.Services.AddCorsPolicy(builder.Configuration);
builder.Services.AddSwaggerDocs();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerDocs();
}

await app.UseDatabaseMigrations();

app.UseHttpsRedirection();
app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
Console.WriteLine("App is starting...");
app.Run();
