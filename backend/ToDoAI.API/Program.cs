using ToDoAI.API.DependencyInjection;
using ToDoAI.API.Extensions;
using ToDoAI.Application.DependencyInjection;
using ToDoAI.Infrastructure.DependencyInjection;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication(builder.Configuration);
builder.Services.AddJwtService();
builder.Services.AddApi();
builder.Services.AddCorsPolicy(builder.Configuration);
builder.Services.AddSwaggerDocs();
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerDocs();
}

await app.UseDatabaseMigrations();

app.UseForwardedHeaders();
app.UseHttpsRedirection();
app.UseCors("Frontend");
app.UseAuthentication();
app.UseCsrfProtection();
app.UseAuthorization();

app.MapControllers();
Console.WriteLine("App is starting...");
app.Run();
