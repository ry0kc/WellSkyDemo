# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

A small home health referral service: an ASP.NET Core 10 API (C#), a VB.NET business-rules library, xUnit tests, and a Vue 3 front end (`referral-ui/`). All data is seeded sample data. The SDK is pinned in `global.json`.

There is no `.sln` file — build and test by pointing at a project. Building `ReferralDemo.Tests` builds everything .NET.

## Commands

```powershell
# Build + test (mirrors CI)
dotnet build ReferralDemo.Tests/ReferralDemo.Tests.csproj
dotnet test ReferralDemo.Tests/ReferralDemo.Tests.csproj

# Single test class / single test
dotnet test ReferralDemo.Tests --filter "FullyQualifiedName~SearchPatientsHandlerTests"
dotnet test ReferralDemo.Tests --filter "FullyQualifiedName~SearchPatientsHandlerTests.Handle_NameSearchIsCaseInsensitive"

# Tests that need no Docker (VB.NET rules + summary handler with fakes)
dotnet test ReferralDemo.Tests --filter "FullyQualifiedName~ReferralRules|FullyQualifiedName~GetReferralSummary"

# Local Postgres, migrations, run API (Swagger at http://localhost:5000/swagger in Development)
docker run --name pg -e POSTGRES_PASSWORD=devpass -p 5432:5432 -d postgres:16
dotnet ef database update --project ReferralDemo
$env:ASPNETCORE_ENVIRONMENT = "Development"; dotnet run --project ReferralDemo

# New migration
dotnet ef migrations add <Name> --project ReferralDemo

# UI (http://localhost:5173; Vite proxies /api to localhost:5000)
cd referral-ui; npm install; npm run dev
npm run build        # runs vue-tsc type-check + vite build (what CI runs)
npm run type-check

# API container
docker build -t referral-api .
```

Integration tests (`[Collection("Postgres")]`) start a real `postgres:16` container via Testcontainers — Docker must be running. `PostgresFixture` applies the app's real migrations (including seed data), so tests assert against the seeded patients/referrals in `ReferralDbContext.OnModelCreating`. Changing seed data will break test expectations.

## Architecture

Request flow: `ReferralsController` → `IMediator.Send` → one MediatR handler per use case → EF Core/Npgsql → PostgreSQL.

- **Thin controller.** `ReferralsController` depends only on `IMediator`; all logic lives in handlers. The request `CancellationToken` is passed through to EF Core.
- **Handlers live next to their request type**, one file per use case in the `ReferralDemo/` root (`SearchPatientsQuery.cs`, `GetReferralStatusQuery.cs`, `ScheduleVisitCommand.cs`). Namespaces don't follow folders: handlers/queries are in `ReferralDemo.Features.Referrals`, entities in `ReferralDemo.Models`, controller in `ReferralDemo.Controllers`.
- **Handler conventions:** each handler opens its own span via `Telemetry.ActivitySource.StartActivity(...)` and tags it; reads use `AsNoTracking()`; writes commit with `SaveChangesAsync`. Handlers needing "today" take a `TimeProvider` (registered as `TimeProvider.System`; tests use `FakeTimeProvider`).
- **MediatR registration** in `Program.cs` scans two assemblies: the API and `ReferralDemo.Rules`. A new handler in either is picked up automatically.
- **C#/VB.NET split (dependency inversion).** `ReferralDemo.Rules` (VB.NET) owns `GetReferralSummaryQuery`, its handler, the pure `ReferralRules` module (overdue / `due_soon` 0–2 days / `on_track`; missing due date → `no_due_date`), and the `IReferralReader` interface. The C# API implements `IReferralReader` with EF Core (`Data/EFReferralReader.cs`). The Rules project must not reference the API project (that would be circular).
- **PostgreSQL specifics:** snake_case identifiers via `UseSnakeCaseNamingConvention()` (must also be applied in any manually built `DbContextOptions`, as `PostgresFixture` does); name search uses `EF.Functions.ILike`; visit timestamps must be UTC (`timestamptz`); `Referral.DueDate` is a `date` column. Because of `ILike` and UTC enforcement, don't switch tests to the in-memory or SQLite providers.
- **Data model:** `Referral` is keyed by `PatientId` (one referral per patient, FK to `Patient`). `Visit` currently has no FK to patients.
- **Config:** connection string `ConnectionStrings:Referrals` in `appsettings.json` (local dev); in containers it is supplied via `ConnectionStrings__Referrals`.
- **Front end:** `referral-ui/src/api.ts` wraps every API call (404 → `null`); types in `src/types.ts`; Pinia store `stores/patients.ts`; views `PatientSearchView` and `PatientDetailView`.

## CI

`.github/workflows/ci.yml` runs on push/PR to `main`: `npm ci && npm run build` in `referral-ui`, and restore/build/test of `ReferralDemo.Tests` in Release. Ubuntu runners have Docker, so the Testcontainers tests run in CI.
