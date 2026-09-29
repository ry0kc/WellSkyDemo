using ReferralDemo.Data;
using ReferralDemo.Features.Referrals;
using Xunit;

namespace ReferralDemo.Tests;

public class GetReferralStatusHandlerTests
{
    [Fact]
    public async Task Handle_PatientWithReferral_ReturnsReferralDetails()
    {
        var data = new InMemoryData();
        var handler = new GetReferralStatusHandler(data);

        var result = await handler.Handle(new GetReferralStatusQuery { PatientId = "P001" }, default);

        Assert.NotNull(result);
        Assert.Equal("Riverside Home Health", result!.Provider);
    }

    [Fact]
    public async Task Handle_PatientWithNoReferralOnFile_ReturnsNull()
    {
        var data = new InMemoryData();
        var handler = new GetReferralStatusHandler(data);

        // P002 (Robert Smith) is discharged, with no referral in the seed
        // data — this is the exact case the controller turns into a 404.
        var result = await handler.Handle(new GetReferralStatusQuery { PatientId = "P002" }, default);

        Assert.Null(result);
    }
}
