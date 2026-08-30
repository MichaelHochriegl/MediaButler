# MediaButler Codex Guidance

MediaButler is a .NET 10 modular monolith for managing media libraries.

Keep this file concise. Detailed repeatable workflows belong in `.agents/skills/`.

## Repository map

### Hosts and shared projects

- `src/AppHost` — .NET Aspire application host and local orchestration.
- `src/Backend` — backend host, application startup, health checks, migration/startup services, and FastEndpoints host configuration.
- `src/Frontend` — Blazor Web App host and shared application UI shell.
- `src/Domain` — shared domain model and domain behavior.
- `src/Persistence` — application `AppDbContext` and application-level EF Core migrations.
- `src/ServiceDefaults` — shared Aspire/service defaults.
- `src/ServiceDiscovery` — shared service/resource descriptors.
- `src/modules/Modules.Common` — infrastructure used to compose MediaButler modules.

### Feature modules

A module may contain:

- `<Module>.Backend` — backend endpoints and backend-specific behavior.
- `<Module>.Contracts` — contracts intentionally shared across boundaries.
- `<Module>.Frontend` — module-specific Blazor UI.
- `<Module>.Persistence` — EF Core configuration and module-specific persistence access.

Use the Libraries module as the current structural reference before creating or restructuring another module.

### Tests

Existing conventions:

- `tests/Domain.Tests.Unit` — domain unit tests.
- `tests/modules/<module>/<Module>.Backend.Tests.Integration` — backend feature integration tests.

Intended layers when corresponding UI/application behavior exists:

- `tests/modules/<module>/<Module>.Frontend.Tests.Component` — bUnit component tests.
- `tests/MediaButler.Tests.E2E` — application-level Aspire tests, with Playwright for critical real-browser flows.

## Architecture

- Keep feature code inside the module that owns it.
- Keep `Backend`, `Frontend`, and `AppHost` focused on composition and host-level concerns.
- Keep domain invariants and domain behavior in `src/Domain`.
- Keep application EF Core infrastructure and migrations in `src/Persistence`.
- Keep module EF configuration and module persistence helpers in the module's persistence project.
- Put genuinely shared module-composition infrastructure in `Modules.Common`; do not turn it into miscellaneous shared code.
- Avoid direct dependencies between feature modules unless intentional and necessary.
- Prefer contracts for intentional boundaries rather than reaching into another module's implementation.

### Module registration

Modules register backend, frontend, persistence, and contract assemblies through the existing `ModuleDescriptors` mechanism. Follow that mechanism rather than manually wiring module implementation details into hosts.

### Backend APIs

The backend uses FastEndpoints.

- Keep endpoints in the owning module backend project.
- Keep cross-boundary request/response types in the module contracts project.
- Let the backend host own global API configuration and endpoint discovery.
- Follow nearby endpoints before introducing a new convention.
- Preserve cancellation through async work.

### Persistence

The application uses a shared `Persistence.AppDbContext`.

- Do not introduce repository abstractions over EF Core.
- `AppDbContext` discovers EF configurations from registered module persistence assemblies.
- Keep `IEntityTypeConfiguration<T>` implementations in the owning module persistence project.
- Use navigation properties and EF Core change tracking where appropriate.
- Keep application migrations in `src/Persistence/Migrations`.
- Treat schema changes and migrations as one feature change.
- Inspect generated migrations for unintended changes.

### Frontend

The frontend is a Blazor Web App using MudBlazor.

- Keep module-specific UI inside the module frontend project.
- Keep `src/Frontend` focused on application shell and composition.
- Follow the MediaButler theme and existing component conventions.
- Avoid custom CSS/JavaScript when Blazor, MudBlazor, or existing abstractions already solve the problem cleanly.

## Testing strategy

Choose the lowest test level that proves the behavior without losing important confidence.

| Behavior | Preferred level |
|---|---|
| Domain invariants, value objects, aggregate behavior | Domain unit test |
| FastEndpoint + DI + validation + EF Core + PostgreSQL | Backend integration test |
| Blazor rendering, interaction, validation, conditional UI | bUnit component test |
| Multi-process AppHost/resource interaction without browser needs | Aspire E2E/integration test |
| Critical real user journey requiring browser behavior | Aspire + Playwright E2E |

Do not duplicate the same assertion at every layer. Each layer should prove something the cheaper layer cannot.

### Existing test stack

Current tests use:

- xUnit v3 on Microsoft Testing Platform (`xunit.v3.mtp-v2`)
- AwesomeAssertions
- FastEndpoints.Testing for backend integration tests
- Testcontainers PostgreSQL for real database integration tests

Follow existing naming and fixture patterns before inventing a competing setup.

### Backend integration tests

The existing pattern uses `AppFixture<Program>` and a real PostgreSQL Testcontainer.

- Exercise features through HTTP/FastEndpoints when practical.
- Keep real application DI and EF Core wiring.
- Reset mutable database state between tests.
- Prefer observable API behavior for assertions.
- Direct `AppDbContext` assertions are acceptable for important persistence postconditions not yet observable through a public endpoint.
- Pass `TestContext.Current.CancellationToken` into async APIs where appropriate.

### Frontend component tests

Use bUnit for component behavior that does not require a real browser.

- Test rendered/user-observable behavior, not MudBlazor internals.
- Register required services/providers in the bUnit context.
- Stub/fake external boundaries such as backend clients.
- Do not use bUnit for full browser behavior that belongs in Playwright.

### End-to-end tests

Use `Aspire.Hosting.Testing` for closed-box tests that launch the AppHost and real resources.

- Treat E2E tests as external clients of the running application.
- Do not reach into backend DI or `DbContext` from Aspire E2E tests.
- Prefer HTTP when a browser adds no confidence.
- Add Playwright only for behavior requiring actual rendered UI/browser interaction.
- Keep browser E2E coverage small and focused on critical journeys.

Use the `media-testing` skill for detailed guidance.

## Engineering constraints

- Target settings from `Directory.Build.props`.
- Warnings are errors; fix rather than suppress without justification.
- Central package versions belong in `Directory.Packages.props`.
- Do not add dependencies casually.
- Do not perform broad cleanup during unrelated feature work.
- Prefer a complete vertical slice over scaffolding hypothetical future layers.

## Validation

Use repository scripts when present:

```bash
./eng/build
./eng/test
./eng/test-domain
./eng/test-backend-integration
./eng/check
```

Run the narrow relevant tests first, then `./eng/check` before completion when practical.

## Skills

- `media-feature` — implementing/changing an application feature.
- `media-testing` — domain, backend integration, bUnit, Aspire, and Playwright test work.
- `media-review` — reviewing a completed change.
