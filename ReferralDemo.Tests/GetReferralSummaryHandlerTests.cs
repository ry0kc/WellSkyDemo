using Microsoft.Extensions.Time.Testing;
using ReferralDemo.Rules;
using Xunit;

namespace ReferralDemo.Tests;

public class GetReferralSummaryHandlerTests
{
    private sealed class FakeReader : IReferralReader
    {
        private readonly ReferralData? _data;
        public FakeReader(ReferralData? data) => _data = data;
        public Task<ReferralData?> GetByPatientIdAsync(string patientId, CancellationToken ct) =>
            Task.FromResult(_data);
    }

    private static readonly FakeTimeProvider Clock =
        new(new DateTimeOffset(2026, 10, 8, 12, 0, 0, TimeSpan.Zero));

    [Fact]
    public async Task Handle_DueTomorrow_IsDueSoon()
    {
        var reader = new FakeReader(new ReferralData { PatientId = "P001", DueDate = new DateTime(2026, 10, 9) });
        var handler = new GetReferralSummaryHandler(reader, Clock);

        var result = await handler.Handle(new GetReferralSummaryQuery { PatientId = "P001" }, default);

        Assert.Equal("due_soon", result.Urgency);
        Assert.Equal(1, result.DaysUntilDue);
    }

    [Fact]
    public async Task Handle_NoReferral_ReturnsNull()
    {
        var handler = new GetReferralSummaryHandler(new FakeReader(null), Clock);

        var result = await handler.Handle(new GetReferralSummaryQuery { PatientId = "P002" }, default);

        Assert.Null(result);
    }
}