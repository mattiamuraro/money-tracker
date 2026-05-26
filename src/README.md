# Money Tracker

A modular personal finance application built on .NET 10 to track money flows, keep domain logic clean, and evolve safely through tests.

> Built with care, tested with discipline, and yes—complex enough to prove that geeks just wanna have fun.

## Why this project

Money Tracker is designed to:
- Keep API endpoints thin and push behavior into Business Logic slices.
- Separate domain/data concerns for maintainability and testability.
- Support local development with a dedicated migration service and worker processes.

## Tech stack

- **Backend:** .NET 10
- **Architecture style:** Vertical Slice Architecture (VSA)
- **Data access:** Entity Framework (separate data and EF projects)
- **Frontend:** Dedicated frontend project (`MoneyTracker.Frontend`)
- **Orchestration/dev host:** `MoneyTracker.AppHost`
- **Workers:**
  - `MoneyTracker.ReconciliationWorker`
  - `MoneyTracker.Data.MigrationService`

## Solution layout

- `MoneyTracker.Api` — HTTP API surface
- `MoneyTracker.BusinessLogic` — application use cases and slice logic
- `MoneyTracker.Data` — data abstractions/contracts
- `MoneyTracker.Data.EntityFramework` — EF Core persistence implementation
- `MoneyTracker.Data.MigrationService` — migration/seeding execution service
- `MoneyTracker.ReconciliationWorker` — background reconciliation processing
- `MoneyTracker.ServiceDefaults` — shared service configuration defaults
- `*.Tests` projects — unit/integration test coverage per layer

## Prerequisites

- .NET 10 SDK
- A local database instance compatible with the configured EF provider
- Visual Studio 2026+ (recommended) or compatible CLI workflow

## Quickstart

From the `src` folder:

1. Restore packages
   - `dotnet restore MoneyTracker.slnx`
2. Build solution
   - `dotnet build MoneyTracker.slnx`
3. Run the app host (recommended local entry point)
   - `dotnet run --project MoneyTracker.AppHost`

If your workflow runs services individually, start API and required worker/migration projects separately.

## .NET Aspire orchestration

This solution uses an Aspire-style local orchestration entry point via `MoneyTracker.AppHost`.

What this gives you:
- Central startup of app components for local development.
- Consistent service wiring through shared defaults (`MoneyTracker.ServiceDefaults`).
- Easier diagnostics when running API, workers, and infrastructure together.

Recommended local flow with Aspire:
1. Run `dotnet run --project MoneyTracker.AppHost`.
2. Verify all required services become healthy.
3. Use the exposed endpoints/UI from the orchestrated environment.

If you need to debug a single service deeply, you can still run projects individually.

## Configuration

Configuration is loaded from standard .NET configuration sources, such as:
- `appsettings.json`
- `appsettings.Development.json`
- Environment variables

Set connection strings and secrets using local user secrets or environment variables (do not commit secrets).

## Database migrations and seed data

Use the migration service to apply migrations and initialize required reference/context data for local environments.

Typical flow:
1. Ensure database connection settings are valid.
2. Run the migration service project.
3. Start API and workers.

## Testing

Run all tests:
- `dotnet test MoneyTracker.slnx`

Or run per project, for example:
- `dotnet test MoneyTracker.BusinessLogic.Tests`
- `dotnet test MoneyTracker.Data.MigrationService.UnitTests`

## Contributing

- Keep changes focused and small.
- Follow existing slice and project boundaries.
- Add/update tests for behavior changes.
- Prefer clear DTO naming (use `Dto` suffix for transfer models).

## Troubleshooting

- **Build fails after pulling changes:** run `dotnet restore` then rebuild.
- **Database errors on startup:** verify connection strings and run migration service first.
- **Worker not processing expected data:** ensure dependent services and migration state are healthy.

---

If you are new to the repo, start with `MoneyTracker.AppHost` for local orchestration, then inspect `MoneyTracker.BusinessLogic` to understand feature slices.