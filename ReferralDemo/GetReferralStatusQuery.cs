using Microsoft.EntityFrameworkCore;
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
    private readonly ReferralDbContext _db;

    public GetReferralStatusHandler(ReferralDbContext db) => _db = db;

    public async Task<Referral?> Handle(GetReferralStatusQuery request, CancellationToken ct)
    {
        using var activity = Telemetry.ActivitySource.StartActivity("GetReferralStatus");
        activity?.SetTag("patient.id", request.PatientId);

        // Read-only query, so skip change tracking.
        var referral = await _db.Referrals
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.PatientId == request.PatientId, ct);

        activity?.SetTag("referral.found", referral is not null);
        return referral;
    }
}
