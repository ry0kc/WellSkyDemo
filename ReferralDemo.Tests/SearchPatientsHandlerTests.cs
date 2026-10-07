using Microsoft.EntityFrameworkCore;
using ReferralDemo.Features.Referrals;
using Xunit;

namespace ReferralDemo.Tests;

// Runs against a real Postgres container. The in-memory provider and SQLite
// can't execute EF.Functions.ILike, so they would test the wrong thing.
[Collection("Postgres")]
public class SearchPatientsHandlerTests
{
    private readonly PostgresFixture _fixture;

    public SearchPatientsHandlerTests(PostgresFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Handle_FiltersByNameContains_ReturnsMatchingPatients()
    {
        await using var db = _fixture.CreateContext();
        var handler = new SearchPatientsHandler(db);

        var result = await handler.Handle(new SearchPatientsQuery { NameContains = "Smith" }, default);

        // Alice Smith, Robert Smith, James Smith
        Assert.Equal(3, result.Count);
        Assert.All(result, p => Assert.Contains("Smith", p.Name));
    }

    [Fact]
    public async Task Handle_NameSearchIsCaseInsensitive()
    {
        await using var db = _fixture.CreateContext();
        var handler = new SearchPatientsHandler(db);

        // Postgres is case-sensitive by default; this proves ILIKE is doing its job.
        var result = await handler.Handle(new SearchPatientsQuery { NameContains = "smith" }, default);

        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task Handle_FiltersByStatus_ReturnsOnlyMatchingStatus()
    {
        await using var db = _fixture.CreateContext();
        var handler = new SearchPatientsHandler(db);

        var result = await handler.Handle(new SearchPatientsQuery { Status = "in_care" }, default);

        Assert.NotEmpty(result);
        Assert.All(result, p => Assert.Equal("in_care", p.Status));
    }

    [Fact]
    public async Task Handle_NoFiltersProvided_ReturnsAllPatients()
    {
        await using var db = _fixture.CreateContext();
        var handler = new SearchPatientsHandler(db);

        var result = await handler.Handle(new SearchPatientsQuery(), default);

        Assert.Equal(await db.Patients.CountAsync(), result.Count);
    }
}