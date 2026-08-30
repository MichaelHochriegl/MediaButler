---
name: media-testing
description: Design, add, repair, or review MediaButler tests. Use for domain unit tests, FastEndpoints/PostgreSQL backend integration tests, bUnit Blazor component tests, Aspire distributed-application tests, Playwright browser E2E tests, regression tests, and deciding which test level a feature needs.
---

# MediaButler testing workflow

Choose the cheapest test level that proves the important behavior while preserving confidence in the real integration points.

Before writing tests, inspect the closest existing tests and their project file.

## Test selection

- **Domain unit** — invariants, value objects, aggregate state transitions, deterministic domain behavior.
- **Backend integration** — FastEndpoints routing/validation/contracts, application DI, EF Core mapping/querying, PostgreSQL persistence.
- **bUnit component** — Blazor rendering/state/forms/events/validation/UI branching without a real browser.
- **Aspire E2E** — interactions across separately running application processes/resources.
- **Aspire + Playwright** — a small number of critical journeys that genuinely require a browser.

Do not duplicate identical assertions across layers.

## Existing conventions

MediaButler currently uses:

- xUnit v3 through `xunit.v3.mtp-v2`
- Microsoft Testing Platform
- AwesomeAssertions
- FastEndpoints.Testing
- Testcontainers PostgreSQL

Preserve these conventions unless the task explicitly changes the stack.

## Read the relevant reference

For substantial work, read only the needed reference(s):

- `references/domain-unit.md`
- `references/backend-integration.md`
- `references/frontend-bunit.md`
- `references/e2e-aspire-playwright.md`

## Regression workflow

1. Reproduce the behavior at the lowest meaningful layer.
2. Confirm the test would fail without the fix when practical.
3. Implement/finalize the fix.
4. Confirm the test passes.
5. Run the broader relevant suite.
6. Keep the regression test about behavior, not incidental implementation.

## Validation

```bash
./eng/test-domain
./eng/test-backend-integration
./eng/test
./eng/check
```

For future component/E2E projects, use direct `dotnet test <project>` while iterating and keep them in solution-wide validation.

Report environmental prerequisites that prevent execution, such as unavailable container runtime or missing Playwright browser binaries.
