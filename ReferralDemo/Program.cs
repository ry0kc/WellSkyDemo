// Program.cs
//
// This is the ONE place MediatR gets registered with the DI container.
// After this, every controller can just ask for IMediator in its
// constructor and MediatR figures out which handler to route to,
// based on the request type — same reflection-based discovery
// mechanism we talked through conceptually.

using MediatR;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using ReferralDemo;
using ReferralDemo.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Tracing setup. Two sources of spans are combined here:
//   1. AddAspNetCoreInstrumentation() — automatic, zero-effort spans for
//      every incoming HTTP request (method, path, status code, duration)
//   2. AddSource(Telemetry.ServiceName) — opts in to YOUR manual spans,
//      the ones created inside the MediatR handlers below
// AddConsoleExporter() prints every finished span straight to the
// console — no GCP account, no Jaeger/Zipkin setup needed to see this work.
builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService(Telemetry.ServiceName))
    .WithTracing(tracing => tracing
        .AddSource(Telemetry.ServiceName)
        .AddAspNetCoreInstrumentation()
        .AddConsoleExporter());


// This line scans the current assembly for every class implementing
// IRequestHandler<,> and registers them all with the DI container
// automatically — you never manually register GetReferralStatusHandler,
// ScheduleVisitHandler, etc. one by one.
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(
    typeof(Program).Assembly,
    typeof(ReferralDemo.Rules.GetReferralSummaryHandler).Assembly));

builder.Services.AddScoped<ReferralDemo.Rules.IReferralReader, EfReferralReader>();
builder.Services.AddSingleton(TimeProvider.System);

// Singleton = one shared instance for the whole app's lifetime.
// Fine for this in-memory demo; a real app would use a Scoped DbContext instead.
builder.Services.AddDbContext<ReferralDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Referrals"))
           .UseSnakeCaseNamingConvention());

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(); // browse to /swagger to interact with this API live
}

app.MapControllers();
app.Run();
