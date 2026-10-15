using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace ReferralDemo.Tests;

// End-to-end through the real HTTP pipeline: routing, the controller,
// MediatR's ValidationBehavior, and UseExceptionHandler. The other tests
// call those pieces directly, so only this one proves they are wired up.
[Collection("Postgres")]
public class ScheduleVisitEndpointTests
{
    private readonly PostgresFixture _fixture;

    public ScheduleVisitEndpointTests(PostgresFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task PostScheduleVisit_InvalidRequest_Returns400WithErrors()
    {
        // WebApplicationFactory boots Program in memory (no port opened);
        // UseSetting overrides appsettings.json so the app uses the
        // Testcontainers database instead of localhost.
        await using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(b => b.UseSetting("ConnectionStrings:Referrals", _fixture.ConnectionString));
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/referrals/schedule-visit",
            new { patientId = "P999", visitDate = "2020-01-01T00:00:00Z" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(["Patient 'P999' not found."], problem.Errors["PatientId"]);
        Assert.Equal(["Visit date cannot be in the past."], problem.Errors["VisitDate"]);
    }
}
