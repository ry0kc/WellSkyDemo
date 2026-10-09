# MediatR/CQRS/Vue Demo 


## Prerequisites

You'll need the .NET SDK installed. Check with:
```powershell
dotnet --version
```
If that's not recognized, install the .NET 8 SDK from
https://dotnet.microsoft.com/download — the free SDK, not just the runtime.

## Setup

**1. Restore packages** (downloads MediatR and Swashbuckle)
```powershell
cd ReferralDemo
dotnet restore
```

**2. Run it**
```powershell
dotnet run
```
Watch the console output for the actual URL it's listening on

**3. Open Swagger** — browse to `<that-url>/swagger`. This gives you an
interactive UI to actually call the endpoints, no separate tool needed.

## What to try

- **`GET /api/referrals/search?name=Smith`** — returns Alice, Robert, and
  James Smith (same LINQ filtering pattern from the core C# rundown)
- **`GET /api/referrals/search?status=in_care`** — filter by status instead
- **`GET /api/referrals/P001/status`** — Alice's referral status (Riverside
  Home Health). Try `P002` — no referral on file, returns 404, since
  Robert was discharged with no active referral.
- **`POST /api/referrals/schedule-visit`** with body:
  ```json
  { "patientId": "P001", "visitDate": "2026-10-05" }
  ```
  Returns a new `visitId` (a `Guid`, generated client-side the moment the
  `Visit` object was constructed 

## If `dotnet restore` complains about package versions

If `MediatR 12.4.1` or `Swashbuckle.AspNetCore 6.6.2` aren't found, just remove 
the `Version=` pin from `ReferralDemo.csproj` and instead run:
```powershell
dotnet add package MediatR
dotnet add package Swashbuckle.AspNetCore
```
That'll fetch whatever the current latest versions actually are — the code
itself doesn't depend on any version-specific features beyond the
`AddMediatR(cfg => ...)` v12-style registration used in `Program.cs`.

## Seeing tracing work (OpenTelemetry, console exporter)

No new setup needed beyond a fresh `dotnet restore` to pull the three new
OpenTelemetry packages. Run it the same way:
```powershell
$env:ASPNETCORE_ENVIRONMENT = "Development"; dotnet run
```

Now hit `GET /api/referrals/search?name=Smith` (via Swagger or curl).
Alongside the normal request logs, you'll see a block of span output printed
to the console — look for two nested entries for that one request:

- One span for the incoming HTTP request itself (created automatically by
  `AddAspNetCoreInstrumentation()` 
- A child span called `SearchPatients`,
  carrying tags like `search.name_contains: Smith` and
  `search.result_count: 3`

That parent/child relationship is automatic — .NET's `Activity` API tracks
the "currently running" activity, so starting a new one inside a handler
that's running underneath an already-started HTTP-request activity nests it
as a child with zero manual wiring. This is what makes a trace an actual
tree instead of a flat list of unrelated log lines.

Try `POST /api/referrals/schedule-visit` too — its span carries the new
`visit.id` as a tag, generated fresh each call, directly visible in the
span output.

## What's deliberately simplified, and why that's fine to say out loud

- EF and PostgreSQL —  `DbContext` injected 
  
- **No validation pipeline behavior** — a real MediatR setup often adds a
  `IPipelineBehavior<TRequest, TResponse>` for automatic validation (often
  paired with FluentValidation) before a handler ever runs. 

## Tracing

- Added OpenTelemetry to this project. Automatic spans for incoming
  HTTP requests via AddAspNetCoreInstrumentation, plus manual spans inside
  each MediatR handler using a shared ActivitySource. Because .NET's
  Activity API tracks the current activity automatically, the handler
  spans nest as children of the request span with no manual parent-linking
  code
- I used the console exporter to see this working without needing GCP
  credentials set up. In production I'd swap that for the Cloud Trace
  exporter, and the instrumentation code itself wouldn't change at all,
  since OpenTelemetry is vendor-neutral by design.
- I tagged each span with business-meaningful data — result counts,
  whether a referral was found, the generated visit ID

- I took the CQRS pattern I understood conceptually and actually built
  a thin controller that only depends on `IMediator`, with Commands
  and Queries as separate classes routed to separate handlers by MediatR's
  reflection-based discovery. The controller has zero idea how any request
  actually gets fulfilled, which is the whole point of the separation.
- I kept the data layer in-memory for this demo originally — but the handler shape
  doesn't change at all once I swapped in a real `DbContext`, which is exactly
  the point of pulling logic out of the controller in the first place.
- To Do: A validation pipeline behavior,
  proper error handling instead of a bare 404
