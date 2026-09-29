using ReferralDemo.Data;
using ReferralDemo.Features.Referrals;
using Xunit;

namespace ReferralDemo.Tests;

public class ScheduleVisitHandlerTests
{
    [Fact]
    public async Task Handle_ValidCommand_CreatesVisitInDataStore()
    {
        var data = new InMemoryData();
        var handler = new ScheduleVisitHandler(data);
        var command = new ScheduleVisitCommand { PatientId = "P001", VisitDate = DateTime.UtcNow };

        var visitId = await handler.Handle(command, default);

        Assert.NotEqual(Guid.Empty, visitId);
        Assert.Single(data.Visits); // this is the actual side effect, not just the return value
        Assert.Equal("P001", data.Visits[0].PatientId);
    }

    [Fact]
    public async Task Handle_CalledTwiceWithSameCommand_CreatesTwoDistinctVisits()
    {
        var data = new InMemoryData();
        var handler = new ScheduleVisitHandler(data);
        var command = new ScheduleVisitCommand { PatientId = "P003", VisitDate = DateTime.UtcNow };

        var firstId = await handler.Handle(command, default);
        var secondId = await handler.Handle(command, default);

        // Worth noting: this handler is NOT idempotent — calling it twice
        // creates two visits, not one. That's correct for THIS operation
        // (scheduling is inherently additive), but it's a good contrast to
        // point out if asked about idempotency: not every write needs to
        // be idempotent, only ones where duplicate delivery is possible
        // and harmful.
        Assert.NotEqual(firstId, secondId);
        Assert.Equal(2, data.Visits.Count);
    }
}
