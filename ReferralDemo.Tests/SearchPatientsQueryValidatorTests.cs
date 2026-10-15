using ReferralDemo.Features.Referrals;
using Xunit;

namespace ReferralDemo.Tests;

public class SearchPatientsQueryValidatorTests
{
    private readonly SearchPatientsQueryValidator _validator = new();

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("in_care")]
    [InlineData("pending_admission")]
    public void Validate_EmptyOrKnownStatus_Passes(string? status)
    {
        var result = _validator.Validate(new SearchPatientsQuery { Status = status });

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_UnknownStatus_Fails()
    {
        var result = _validator.Validate(new SearchPatientsQuery { Status = "bogus" });

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(SearchPatientsQuery.Status));
    }

    [Fact]
    public void Validate_NameOver100Chars_Fails()
    {
        var result = _validator.Validate(new SearchPatientsQuery { NameContains = new string('a', 101) });

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(SearchPatientsQuery.NameContains));
    }
}
