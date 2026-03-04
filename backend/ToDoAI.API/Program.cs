using Microsoft.EntityFrameworkCore;
using ToDoAI.ToDoAI.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

string? connectionString = builder.Configuration.GetConnectionString("Default");

Console.WriteLine(connectionString);

builder.Services.AddDbContext<ToDoAIDbContext>(opts =>
    opts.UseNpgsql(connectionString, o =>
    {
        o.MigrationsAssembly(typeof(ToDoAIDbContext).Assembly.FullName);
        o.MigrationsHistoryTable("__EFMigrationsHistory", "ToDoAIService");
    }));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddAuthentication("Bearer").AddJwtBearer();  
builder.Services.AddAuthorization();   

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ToDoAIDbContext>();
    await db.Database.MigrateAsync();
}

app.UseAuthentication();
app.UseAuthorization();
app.UseHttpsRedirection();


app.Run();
