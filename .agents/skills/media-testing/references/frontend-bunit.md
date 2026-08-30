# Frontend component tests with bUnit

## Intended location

`tests/modules/<module>/<Module>.Frontend.Tests.Component`

The first such project may not exist yet. When creating it, preserve MediaButler's testing conventions:

- xUnit v3 / Microsoft Testing Platform
- AwesomeAssertions
- bUnit

Add package versions through `Directory.Packages.props`.

## What belongs in bUnit

- initial/loading/empty/error rendering
- parameters and cascading values
- forms and validation
- button/event behavior
- conditional rendering
- component state transitions
- injected frontend service/client interaction
- emitted callbacks

bUnit should test real Blazor component lifecycle/behavior without needing a browser.

## Boundaries

Treat the backend as external. Stub/fake the frontend API/client abstraction to deliberately drive success, validation failure, server/network failure, empty results, and loading states.

Do not start the real backend/PostgreSQL solely for component tests; that belongs in E2E.

## MudBlazor

Register required services/providers in the test context. Test MediaButler behavior rather than MudBlazor internals.

Prefer assertions based on visible text, semantic HTML, enabled/disabled state, accessible attributes/roles, and callbacks/state changes. Avoid selectors coupled to generated MudBlazor CSS classes/internal DOM.

Use exact `MarkupMatches` only when exact markup is the contract; otherwise prefer targeted semantic assertions.

## Promote to browser tests when needed

Do not force bUnit to model real layout/CSS, browser focus behavior, whole-app navigation, real browser APIs/JavaScript, or cross-process frontend/backend behavior. Use Aspire + Playwright when those behaviors are important enough.
