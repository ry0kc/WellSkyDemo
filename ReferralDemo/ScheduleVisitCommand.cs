
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
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

public class ScheduleVisitCommandValidator : AbstractValidator<ScheduleVisitCommand>
{
    public ScheduleVisitCommandValidator(ReferralDbContext db, TimeProvider clock)
    {
        // Stop at the first failure so the database is only queried
        // for a well-formed patient ID.
        RuleFor(c => c.PatientId)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .MaximumLength(50)
            .MustAsync((id, ct) => db.Patients.AnyAsync(p => p.Id == id, ct))
            .WithMessage("Patient '{PropertyValue}' not found.");

        RuleFor(c => c.VisitDate)
            .Cascade(CascadeMode.Stop)
            .NotEqual(default(DateTime)).WithMessage("Visit date is required.")
            .Must(d => d.ToUniversalTime() >= clock.GetUtcNow().UtcDateTime)
            .WithMessage("Visit date cannot be in the past.");
    }
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
