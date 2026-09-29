using System.Diagnostics;

namespace ReferralDemo;

// "Activity" is .NET's built-in, framework-level name for what OpenTelemetry
// calls a "span" — OpenTelemetry hooks into System.Diagnostics.Activity
// rather than inventing its own separate tracing primitive. This single,
// shared ActivitySource is what your handlers use to create their own
// custom spans, on top of the automatic ones AddAspNetCoreInstrumentation()
// gives you for free.
public static class Telemetry
{
    public const string ServiceName = "ReferralDemo";
    public static readonly ActivitySource ActivitySource = new(ServiceName);
}
