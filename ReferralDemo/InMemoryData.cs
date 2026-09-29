using ReferralDemo.Models;

namespace ReferralDemo.Data;

// Stands in for a real database (SQL Server + EF Core, in a real build)
// so this demo has zero external dependencies to install or configure.
// Registered as a Singleton in Program.cs, so this same instance and
// its data persists for as long as the app is running.
public class InMemoryData
{
    public List<Patient> Patients { get; } = new()
    {
        new Patient { Id = "P001", Name = "Alice Smith", Status = "in_care" },
        new Patient { Id = "P002", Name = "Robert Smith", Status = "discharged" },
        new Patient { Id = "P003", Name = "Maria Gonzalez", Status = "in_care" },
        new Patient { Id = "P004", Name = "James Smith", Status = "pending_admission" },
    };

    public Dictionary<string, Referral> Referrals { get; } = new()
    {
        ["P001"] = new Referral
        {
            PatientId = "P001",
            Provider = "Riverside Home Health",
            NextStep = "Initial assessment scheduled",
            DueDate = new DateTime(2026, 10, 2),
        },
        ["P003"] = new Referral
        {
            PatientId = "P003",
            Provider = "Sunrise Physical Therapy",
            NextStep = "Awaiting insurance authorization",
            DueDate = new DateTime(2026, 9, 30),
        },
    };

    // Starts empty — this is what ScheduleVisitCommand will actually write to,
    // so you can see a real write happen, not just reads.
    public List<Visit> Visits { get; } = new();
}
