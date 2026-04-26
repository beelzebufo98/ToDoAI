using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using ToDoAI.Infrastructure.Data;

namespace ToDoAI.IntegrationTests.Fixtures;

public sealed class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:17-alpine")
        .Build();

    private ServiceProvider? _serviceProvider;

    public IDbContextFactory<ToDoAIDbContext> DbContextFactory { get; private set; } = default!;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        var services = new ServiceCollection();
        services.AddDbContextFactory<ToDoAIDbContext>(options =>
            options.UseNpgsql(_container.GetConnectionString(), npgsql =>
            {
                npgsql.MigrationsAssembly(typeof(ToDoAIDbContext).Assembly.FullName);
                npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "ToDoAIService");
            }));

        _serviceProvider = services.BuildServiceProvider();
        DbContextFactory = _serviceProvider.GetRequiredService<IDbContextFactory<ToDoAIDbContext>>();

        await using var db = await DbContextFactory.CreateDbContextAsync();
        await db.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        if (_serviceProvider is not null)
            await _serviceProvider.DisposeAsync();
        await _container.DisposeAsync();
    }
}
