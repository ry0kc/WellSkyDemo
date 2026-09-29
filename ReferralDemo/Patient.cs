namespace ReferralDemo.Models;

public class Patient
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    // "in_care", "discharged", "pending_admission" — same statuses
    // as the fake data in the agent demo, kept consistent on purpose.
    public string Status { get; set; } = string.Empty;
}
