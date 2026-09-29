namespace ReferralDemo.Models;

public class Referral
{
    public string PatientId { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string NextStep { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
}

public class Visit
{
    public Guid Id { get; set; }
    public string PatientId { get; set; } = string.Empty;
    public DateTime Date { get; set; }
}
