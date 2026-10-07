using Microsoft.EntityFrameworkCore;
using ReferralDemo.Features.Referrals;
using Xunit;

namespace ReferralDemo.Tests;

[Collection("Postgres")]
public class ScheduleVisitHandlerTests
{
    private readonly PostgresFixture _fixture;

    public ScheduleVisitHandlerTests(PostgresFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Handle_SchedulesVisit_PersistsToDatabase()
    {
        var visitDate = new DateTime(2026, 10, 15, 14, 0, 0, DateTimeKind.Utc);

        Guid id;
        await using (var db = _fixture.CreateContext())
        {
            var handler = new ScheduleVisitHandler(db);
            id = await handler.Handle(
                new ScheduleVisitCommand { PatientId = "P001", VisitDate = visitDate }, default);
        }

        // Read back with a fresh context so we're checking the database,
        // not the first context's in-memory change tracker.
        await using var verify = _fixture.CreateContext();
        var saved = await verify.Visits.SingleAsync(v => v.Id == id);

        Assert.Equal("P001", saved.PatientId);
        Assert.Equal(visitDate, saved.Date);
    }

    [Fact]
    public async Task Handle_TwoVisits_ReturnDistinctIds()
    {
        await using var db = _fixture.CreateContext();
        var handler = new ScheduleVisitHandler(db);
        var command = new ScheduleVisitCommand
        {
            PatientId = "P003",
            VisitDate = new DateTime(2026, 10, 20, 9, 0, 0, DateTimeKind.Utc),
        };

        var first = await handler.Handle(command, default);
        var second = await handler.Handle(command, default);

        Assert.NotEqual(first, second);
    }
}