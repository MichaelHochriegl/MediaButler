# Aspire and Playwright E2E tests

## Intended location

`tests/MediaButler.Tests.E2E`

When introducing it, keep package versions in `Directory.Packages.props` and preserve xUnit v3 / Microsoft Testing Platform.

## Aspire harness

Use `Aspire.Hosting.Testing` and `DistributedApplicationTestingBuilder` to launch the AppHost and resources.

Treat Aspire tests as closed-box tests:

```text
test process
   |
   v
AppHost
  |---- Backend process
  |---- Frontend process
  `---- PostgreSQL/resources
```

Do not resolve backend `AppDbContext` or other process internals from an Aspire E2E test.

## AppHost lifecycle

Starting the complete AppHost is expensive. Share it at a sensible fixture scope when isolation permits, while keeping test data and browser state isolated.

Wait for required resources/endpoints to become ready. Do not sleep for arbitrary durations. Use Aspire resource endpoints/service discovery rather than hard-coded local ports.

## HTTP-level E2E

Not every E2E test needs a browser. Prefer ordinary HTTP clients when testing deployed-like AppHost/resource wiring or multi-process workflows without browser-specific behavior.

## Playwright

Add Playwright only when the behavior requires actual UI interaction.

Because MediaButler uses xUnit v3, prefer Playwright's current xUnit v3 integration when it fits the fixture design, or use `Microsoft.Playwright` directly when explicit browser lifecycle composes more cleanly with the Aspire fixture.

Keep browser coverage to critical journeys, e.g.:

```text
open Libraries page
 -> create physical library through UI
 -> submit
 -> observe success/navigation
 -> verify library appears in UI
```

Do not repeat every backend validation case in the browser.

## Locators/assertions

Prefer role + accessible name, label, visible text, and stable test ids only where semantic locators are impractical. Use Playwright web-first assertions. Never use arbitrary `Task.Delay` as synchronization.

## Isolation and setup

Use a fresh browser context/page per test. Keep server/database state independent. Prefer setup through public/API surfaces rather than process internals.

## Diagnostics

When supported, retain useful failure artifacts: Playwright trace, screenshot, browser console/network details, and Aspire resource logs. These are part of the agent harness and should make failures diagnosable.

## Environment

Playwright requires matching browser binaries (and sometimes OS dependencies). If unavailable, report that the E2E suite could not run rather than calling it passing.
