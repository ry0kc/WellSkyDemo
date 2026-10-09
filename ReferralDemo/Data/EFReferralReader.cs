using Microsoft.EntityFrameworkCore;
using ReferralDemo.Rules;

namespace ReferralDemo.Data;

public class EfReferralReader : IReferralReader
{
    private readonly ReferralDbContext _db;

    public EfReferralReader(ReferralDbContext db) => _db = db;

    public Task<ReferralData?> GetByPatientIdAsync(string patientId, CancellationToken ct) =>
        _db.Referrals
            .AsNoTracking()
            .Where(r => r.PatientId == patientId)
            .Select(r => new ReferralData
            {
                PatientId = r.PatientId,
                Provider = r.Provider,
                NextStep = r.NextStep,
                DueDate = r.DueDate,
            })
            .FirstOrDefaultAsync(ct);
}