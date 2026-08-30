# Backend integration tests

## Location

`tests/modules/<module>/<Module>.Backend.Tests.Integration`

Reference: `tests/modules/libraries/Libraries.Backend.Tests.Integration`.

## Existing harness

Libraries currently uses:

- `FastEndpoints.Testing`
- `AppFixture<Program>`
- real PostgreSQL via `Testcontainers.PostgreSql`
- real backend DI/EF Core wiring
- xUnit v3 / Microsoft Testing Platform
- AwesomeAssertions

Inspect the nearest `*AppFixture.cs` before creating a fixture.

## Preferred shape

```text
HTTP request
  -> FastEndpoints routing
  -> binding / validation
  -> dependency injection
  -> feature behavior
  -> AppDbContext / module persistence
  -> PostgreSQL
  -> HTTP response
```

Assert status, response/problem contract, contractually relevant headers, and important persisted postconditions.

## Database

Use real PostgreSQL for relational behavior. Tests must be repeatable and order-independent.

Follow the established reset strategy. As the schema grows, prefer a centralized maintainable reset mechanism over scattered table-specific SQL.

Do not substitute EF Core InMemory for behavior relying on relational/PostgreSQL semantics.

## State assertions

Prefer public API behavior when available. Direct `AppDbContext` assertions are acceptable for important persistence postconditions not yet observable through a public read path; migrate toward public behavior once such a path exists.

## Cancellation

Pass `TestContext.Current.CancellationToken` to async calls that support cancellation where practical.

## Fixture cost/isolation

Do not start a PostgreSQL container per individual test when an existing fixture can safely share it. Share expensive infrastructure at a suitable scope, then isolate mutable data between tests. Do not disable parallelization merely to hide isolation problems.
