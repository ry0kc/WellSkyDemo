using Microsoft.EntityFrameworkCore;
using ReferralDemo.Models;

namespace ReferralDemo.Data;

public class ReferralDbContext : DbContext
{
    public ReferralDbContext(DbContextOptions<ReferralDbContext> options)
        : base(options) { }

    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Referral> Referrals => Set<Referral>();
    public DbSet<Visit> Visits => Set<Visit>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Patient>(e =>
        {
            e.HasKey(p => p.Id);
            e.HasData(
                new Patient { Id = "P001", Name = "Alice Smith", Status = "in_care" },
                new Patient { Id = "P002", Name = "Robert Smith", Status = "discharged" },
                new Patient { Id = "P003", Name = "Maria Gonzalez", Status = "in_care" },
                new Patient { Id = "P004", Name = "James Smith", Status = "pending_admission" });
        });

        modelBuilder.Entity<Referral>(e =>
        {
            e.HasKey(r => r.PatientId);
            e.HasOne<Patient>()
                .WithOne()
                .HasForeignKey<Referral>(r => r.PatientId);

            // Postgres maps DateTime to timestamptz and rejects
            // non-UTC values; a due date is a calendar date anyway.
            e.Property(r => r.DueDate).HasColumnType("date");

            e.HasData(
                new Referral
                {
                    PatientId = "P001",
                    Provider = "Riverside Home Health",
                    NextStep = "Initial assessment scheduled",
                    DueDate = new DateTime(2026, 10, 2),
                },
                new Referral
                {
                    PatientId = "P003",
                    Provider = "Sunrise Physical Therapy",
                    NextStep = "Awaiting insurance authorization",
                    DueDate = new DateTime(2026, 9, 30),
                });
        });

        modelBuilder.Entity<Visit>();
    }
}