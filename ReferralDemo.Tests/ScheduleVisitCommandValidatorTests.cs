using Microsoft.Extensions.Time.Testing;
using ReferralDemo.Features.Referrals;
using Xunit;

namespace ReferralDemo.Tests;

// The patient-exists rule queries the database, so this runs against Postgres.
[Collection("Postgres")]
public class ScheduleVisitCommandValidatorTests
{
    private static readonly FakeTimeProvider Clock =
        new(new DateTimeOffset(2026, 10, 8, 12, 0, 0, TimeSpan.Zero));

    private static readonly DateTime Tomorrow = new(2026, 10, 9, 9, 0, 0, DateTimeKind.Utc);

    private readonly PostgresFixture _fixture;

    public ScheduleVisitCommandValidatorTests(PostgresFixture fixture) => _fixture = fixture;

    private async Task<FluentValidation.Results.ValidationResult> Validate(ScheduleVisitCommand command)
    {
        await using var db = _fixture.CreateContext();
        return await new ScheduleVisitCommandValidator(db, Clock).ValidateAsync(command);
    }

    [Fact]
    public async Task Validate_KnownPatientFutureDate_Passes()
    {
        var result = await Validate(new ScheduleVisitCommand { PatientId = "P001", VisitDate = Tomorrow });

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_EmptyPatientId_Fails()
    {
        var result = await Validate(new ScheduleVisitCommand { PatientId = "", VisitDate = Tomorrow });

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ScheduleVisitCommand.PatientId));
    }

    [Fact]
    public async Task Validate_UnknownPatient_Fails()
    {
        var result = await Validate(new ScheduleVisitCommand { PatientId = "P999", VisitDate = Tomorrow });

        var error = Assert.Single(result.Errors);
        Assert.Equal("Patient 'P999' not found.", error.ErrorMessage);
    }

    [Fact]
    public async Task Validate_MissingDate_Fails()
    {
        var result = await Validate(new ScheduleVisitCommand { PatientId = "P001" });

        var error = Assert.Single(result.Errors);
        Assert.Equal("Visit date is required.", error.ErrorMessage);
    }

    [Fact]
    public async Task Validate_PastDate_Fails()
    {
        var yesterday = new DateTime(2026, 10, 7, 9, 0, 0, DateTimeKind.Utc);

        var result = await Validate(new ScheduleVisitCommand { PatientId = "P001", VisitDate = yesterday });

        var error = Assert.Single(result.Errors);
        Assert.Equal("Visit date cannot be in the past.", error.ErrorMessage);
    }
}
