using ReferralDemo.Rules;
using Xunit;

namespace ReferralDemo.Tests;

public class ReferralRulesTests
{
    private static readonly DateTime Today = new(2026, 10, 8);

    [Theory]
    [InlineData(-1, "overdue")]
    [InlineData(0, "due_soon")]
    [InlineData(2, "due_soon")]
    [InlineData(3, "on_track")]
    public void Urgency_ClassifiesByDaysUntilDue(int daysFromToday, string expected)
    {
        var result = ReferralRules.Urgency(Today.AddDays(daysFromToday), Today);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void IsOverdue_IgnoresTimeOfDay()
    {
        var dueLaterToday = Today.AddHours(23);
        Assert.False(ReferralRules.IsOverdue(dueLaterToday, Today.AddHours(1)));
    }
}