using Microsoft.EntityFrameworkCore;
using ReferralDemo.Data;
using Testcontainers.PostgreSql;
using Xunit;

namespace ReferralDemo.Tests;

public class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container =
    new PostgreSqlBuilder("postgres:16").Build();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        // Applies the same migrations production uses, including seed data,
        // so the tests also prove the migrations work.
        await using var db = CreateContext();
        await db.Database.MigrateAsync();
    }

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();

    public ReferralDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<ReferralDbContext>()
            .UseNpgsql(_container.GetConnectionString())
            .UseSnakeCaseNamingConvention()
            .Options);
}

// Shares one container across every test class marked [Collection("Postgres")].
[CollectionDefinition("Postgres")]
public class PostgresCollection : ICollectionFixture<PostgresFixture> { }