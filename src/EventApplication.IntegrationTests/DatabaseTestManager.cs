using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Xunit;

namespace EventApplication.IntegrationTests;

public class DatabaseTestManager : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .Build();
    
    public async ValueTask InitializeAsync()
    {
        await _postgres.StartAsync();
        await ResetDatabaseAsync();
    }
    
    public async ValueTask DisposeAsync()
    {
        await _postgres.DisposeAsync();
    }
    
    protected async Task<AppDbContext> CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            .Options;

        var context = new AppDbContext(options);
        await context.Database.MigrateAsync();
        return context;
    }
    
    protected async Task ResetDatabaseAsync()
    {
        await using var context = await CreateContext();
        await context.Database.ExecuteSqlRawAsync(
            """TRUNCATE TABLE "Bookings", "Events", "Users" RESTART IDENTITY CASCADE""");
    }
}