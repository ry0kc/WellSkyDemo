
using MediatR;
using ReferralDemo.Data;
using ReferralDemo.Models;
using ReferralDemo;

namespace ReferralDemo.Features.Referrals;

// This is a COMMAND — it changes state. Notice it's a separate class
// from the queries above, even though both go through the same
// _mediator.Send(...) call site in the controller. That separation
// IS Command/Query Responsibility Segregation, in its simplest form.
public class ScheduleVisitCommand : IRequest<Guid>
{
    public string PatientId { get; set; } = string.Empty;
    public DateTime VisitDate { get; set; }
}

public class ScheduleVisitHandler : IRequestHandler<ScheduleVisitCommand, Guid>
{
    private readonly ReferralDbContext _db;

    public ScheduleVisitHandler(ReferralDbContext db) => _db = db;

    public async Task<Guid> Handle(ScheduleVisitCommand request, CancellationToken ct)
    {
        using var activity = Telemetry.ActivitySource.StartActivity("ScheduleVisit");
        activity?.SetTag("patient.id", request.PatientId);
        activity?.SetTag("visit.date", request.VisitDate.ToString("O"));

        var visit = new Visit
        {
            Id = Guid.NewGuid(),
            PatientId = request.PatientId,
            // Postgres timestamptz only accepts UTC DateTimes.
            Date = request.VisitDate.ToUniversalTime(),
        };

        _db.Visits.Add(visit);
        await _db.SaveChangesAsync(ct);

        activity?.SetTag("visit.id", visit.Id.ToString());
        return visit.Id;
    }
}
