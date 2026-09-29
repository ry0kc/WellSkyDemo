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
    private readonly InMemoryData _data;

    public SearchPatientsHandler(InMemoryData data) => _data = data;

    public Task<List<Patient>> Handle(SearchPatientsQuery request, CancellationToken ct)
    {
        // StartActivity begins a new span. Because AddAspNetCoreInstrumentation()
        // already started a span for the incoming HTTP request, .NET's Activity
        // API automatically nests this one as a CHILD of that request span —
        // no manual parent-linking code needed. This is what makes a trace a
        // TREE, not just a flat list of unrelated events.
        using var activity = Telemetry.ActivitySource.StartActivity("SearchPatients");
        activity?.SetTag("search.name_contains", request.NameContains);
        activity?.SetTag("search.status", request.Status);

        // Same LINQ .Where() filtering pattern from the core C# rundown —
        // reads like SQL WHERE clauses chained together.
        var results = _data.Patients.Where(p =>
            (string.IsNullOrEmpty(request.NameContains) ||
             p.Name.Contains(request.NameContains, StringComparison.OrdinalIgnoreCase)) &&
            (string.IsNullOrEmpty(request.Status) || p.Status == request.Status)
        ).ToList();

        activity?.SetTag("search.result_count", results.Count);

        return Task.FromResult(results);
    }
}
