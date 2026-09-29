using MediatR;
using ReferralDemo.Data;
using ReferralDemo.Models;
using ReferralDemo;

namespace ReferralDemo.Features.Referrals;

public class GetReferralStatusQuery : IRequest<Referral?>
{
    public string PatientId { get; set; } = string.Empty;
}

public class GetReferralStatusHandler : IRequestHandler<GetReferralStatusQuery, Referral?>
{
    private readonly InMemoryData _data;

    public GetReferralStatusHandler(InMemoryData data) => _data = data;

    public Task<Referral?> Handle(GetReferralStatusQuery request, CancellationToken ct)
    {
        using var activity = Telemetry.ActivitySource.StartActivity("GetReferralStatus");
        activity?.SetTag("patient.id", request.PatientId);

        // TryGetValue on a Dictionary — O(1) lookup, no exception if missing,
        // just returns false and leaves 'referral' as null.
        _data.Referrals.TryGetValue(request.PatientId, out var referral);

        // A tag showing a real, meaningful business outcome — not just
        // "did the code run," but "was a referral actually found." This is
        // exactly the kind of detail that makes a trace genuinely useful
        // for debugging later, versus a trace that only proves code executed.
        activity?.SetTag("referral.found", referral is not null);

        return Task.FromResult(referral);
    }
}
