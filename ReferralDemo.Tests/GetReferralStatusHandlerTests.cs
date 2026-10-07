using ReferralDemo.Features.Referrals;
using Xunit;

namespace ReferralDemo.Tests;

[Collection("Postgres")]
public class GetReferralStatusHandlerTests
{
    private readonly PostgresFixture _fixture;

    public GetReferralStatusHandlerTests(PostgresFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Handle_PatientWithReferral_ReturnsReferral()
    {
        await using var db = _fixture.CreateContext();
        var handler = new GetReferralStatusHandler(db);

        var result = await handler.Handle(new GetReferralStatusQuery { PatientId = "P001" }, default);

        Assert.NotNull(result);
        Assert.Equal("Riverside Home Health", result.Provider);
    }

    [Fact]
    public async Task Handle_PatientWithoutReferral_ReturnsNull()
    {
        await using var db = _fixture.CreateContext();
        var handler = new GetReferralStatusHandler(db);

        // P002 is seeded as a patient but has no referral.
        var result = await handler.Handle(new GetReferralStatusQuery { PatientId = "P002" }, default);

        Assert.Null(result);
    }
}