using ReferralDemo.Data;
using ReferralDemo.Features.Referrals;
using Xunit;

namespace ReferralDemo.Tests;

// Notice what's NOT here: no mocking framework, no fake database setup.
// InMemoryData is a plain, concrete class with no external dependencies,
// so we just instantiate a fresh one per test — real unit isolation
// without needing Moq for this particular handler.
public class SearchPatientsHandlerTests
{
    [Fact]
    public async Task Handle_FiltersByNameContains_ReturnsMatchingPatients()
    {
        var data = new InMemoryData();
        var handler = new SearchPatientsHandler(data);

        var result = await handler.Handle(new SearchPatientsQuery { NameContains = "Smith" }, default);

        // Alice Smith, Robert Smith, James Smith — 3 of the 4 seeded patients
        Assert.Equal(3, result.Count);
        Assert.All(result, p => Assert.Contains("Smith", p.Name));
    }

    [Fact]
    public async Task Handle_FiltersByStatus_ReturnsOnlyMatchingStatus()
    {
        var data = new InMemoryData();
        var handler = new SearchPatientsHandler(data);

        var result = await handler.Handle(new SearchPatientsQuery { Status = "in_care" }, default);

        Assert.All(result, p => Assert.Equal("in_care", p.Status));
    }

    [Fact]
    public async Task Handle_NoFiltersProvided_ReturnsAllPatients()
    {
        var data = new InMemoryData();
        var handler = new SearchPatientsHandler(data);

        // This is the specific behavior we reasoned through earlier: an
        // empty/null filter should match everything, via the
        // IsNullOrEmpty(...) || ... short-circuit in the handler.
        var result = await handler.Handle(new SearchPatientsQuery(), default);

        Assert.Equal(data.Patients.Count, result.Count);
    }
}
