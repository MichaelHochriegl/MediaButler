---
name: media-feature
description: Implement or change a MediaButler application feature across its modular .NET domain, backend, contracts, persistence, and Blazor frontend. Use for endpoints, workflows, persistence changes, UI changes, and complete vertical slices. Do not use for pure review or test-only work.
---

# MediaButler feature workflow

Implement the smallest complete vertical slice that satisfies the requested behavior while preserving MediaButler's boundaries.

## 1. Establish the boundary

Before editing:

1. Identify the owning module.
2. Inspect nearby features and tests.
3. Inspect relevant domain types and module registration.
4. Determine which parts actually change: domain, contracts, backend, module persistence, migration, frontend.
5. Identify public API, persisted-schema, and cross-module compatibility impacts.

Do not create layers merely because a theoretical architecture could contain them.

## 2. Preserve ownership

- `src/Domain` owns domain behavior/invariants.
- `<Module>.Backend` owns backend implementation.
- `<Module>.Contracts` owns intentionally shared boundary types.
- `<Module>.Frontend` owns module UI.
- `<Module>.Persistence` owns module EF configuration/helpers.
- `src/Persistence` owns application `AppDbContext` and migrations.
- Host projects compose modules and should contain minimal feature-specific behavior.

Use Libraries as the structural reference for new modules unless a deliberate deviation is required. Avoid feature-specific code in `Modules.Common`.

## 3. Domain

When behavior belongs to the domain:

- Keep invariants in the appropriate value object/entity/aggregate.
- Make invalid states hard to construct where practical.
- Avoid transport/EF concerns in domain behavior.
- Add focused domain unit tests for meaningful invariant or state-transition changes.

## 4. Backend/API

MediaButler uses FastEndpoints.

- Inspect a nearby endpoint first.
- Keep endpoint implementation in the owning module backend.
- Put boundary-facing request/response types in Contracts.
- Rely on host-level FastEndpoints conventions.
- Preserve `CancellationToken` through async work.
- Return meaningful HTTP behavior rather than persistence implementation details.

Do not introduce MediatR merely to wrap FastEndpoints.

## 5. Persistence

- Put module EF configuration in `<Module>.Persistence`.
- Follow persistence assembly registration/discovery.
- Use `AppDbContext` directly; no repository/unit-of-work wrapper.
- Use navigation properties/change tracking where appropriate.
- Put generated migrations under `src/Persistence/Migrations`.
- Inspect migrations for unrelated schema changes.

## 6. Frontend

- Keep module UI in `<Module>.Frontend`.
- Reuse MudBlazor and the existing theme.
- Follow the established frontend API-client pattern.
- Handle relevant loading/empty/success/validation/failure states.
- Avoid JS interop unless Blazor/MudBlazor cannot reasonably provide the behavior.
- Add bUnit coverage for non-trivial component behavior.
- Add browser E2E only for critical journeys needing real-browser confidence.

## 7. Choose tests deliberately

- domain invariant/behavior -> domain unit
- endpoint + DI + EF Core + PostgreSQL -> backend integration
- Blazor rendering/interaction -> bUnit component
- distributed AppHost/resource behavior -> Aspire E2E
- critical browser journey -> Aspire + Playwright E2E

Do not test identical behavior at every level. Use `media-testing` for substantial test work.

## 8. Validate

Run focused tests first, then:

```bash
./eng/check
```

Then inspect:

```bash
git status --short
git diff --check
git diff
```

## 9. Report

Summarize behavior, key design choices, tests added/changed, checks actually run, and real remaining limitations. Never claim tests passed if they were not run.
