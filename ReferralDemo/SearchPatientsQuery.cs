using Microsoft.EntityFrameworkCore;
using MediatR;
using ReferralDemo.Data;
using ReferralDemo.Models;
using ReferralDemo;

namespace ReferralDemo.Features.Referrals;

// This is a QUERY — read-only, no side effects. Notice both properties
// are optional (nullable) so you can search by name, by status, or both.
public class SearchPatientsQuery : IRequest<List<Patient>>
{
    public string? NameContains { get; set; }
    public string? Status { get; set; }
}

public class SearchPatientsHandler : IRequestHandler<SearchPatientsQuery, List<Patient>>
{
    private readonly ReferralDbContext _db;

    public SearchPatientsHandler(ReferralDbContext db) => _db = db;

    public async Task<List<Patient>> Handle(SearchPatientsQuery request, CancellationToken ct)
    {
        using var activity = Telemetry.ActivitySource.StartActivity("SearchPatients");
        activity?.SetTag("search.name_contains", request.NameContains);
        activity?.SetTag("search.status", request.Status);

        IQueryable<Patient> query = _db.Patients.AsNoTracking();

        // Build the WHERE clause only from filters actually supplied.
        if (!string.IsNullOrEmpty(request.NameContains))
            query = query.Where(p => EF.Functions.ILike(p.Name, $"%{request.NameContains}%"));

        if (!string.IsNullOrEmpty(request.Status))
            query = query.Where(p => p.Status == request.Status);

        var results = await query.ToListAsync(ct);

        activity?.SetTag("search.result_count", results.Count);
        return results;
    }
}
