---
name: media-review
description: Review MediaButler changes for correctness, regressions, architecture violations, persistence/API mistakes, insufficient test coverage, UI testing mistakes, and unnecessary complexity. Use for diffs, commits, branches, or final self-review.
---

# MediaButler change review

Review as if the change were going into `main`. Prioritize correctness and architectural fit over style preferences.

## Establish scope

```bash
git status --short
git diff --check
git diff
```

Use the appropriate diff when reviewing a commit/branch/staged work.

## Review priorities

### Correctness

Look for incomplete behavior, edge cases, cancellation/nullability issues, concurrency problems, misleading errors, and accidental changes.

### Architecture

Check that domain behavior is in `src/Domain`, feature code is in the owning module, hosts remain composition-focused, module EF configuration is in `<Module>.Persistence`, migrations remain in `src/Persistence/Migrations`, and `Modules.Common` is not becoming a dumping ground.

### API

Check route/verb, request/response contract, validation/Problem Details, cancellation, global-vs-local configuration, and compatibility impact.

### EF Core/database

Check relationship/cardinality, configuration ownership, query shape, constraints, migration correctness, and absence of unnecessary repository abstractions.

### Frontend

Check loading/empty/error/success states, module ownership, MudBlazor conventions, unnecessary CSS/JS, contract consistency, and user-visible/accessibility behavior.

### Tests

Ask which levels are actually warranted: domain unit, backend integration, bUnit, Aspire E2E, Playwright E2E. Do not demand every layer.

Flag:

- mocked persistence replacing the established PostgreSQL integration harness
- backend integration tests bypassing HTTP when HTTP behavior is under test
- bUnit tests coupled to MudBlazor internal classes/DOM
- browser E2E tests duplicating cheaper coverage
- Aspire E2E tests reaching into backend DI/DbContext
- arbitrary sleeps instead of readiness/web-first assertions
- state leakage/order dependence

### Complexity

Flag speculative abstractions, frameworks for one use case, unrelated refactors, duplicated infrastructure, and competing patterns without clear benefit.

## Validate

When safe:

```bash
./eng/check
```

Report findings in descending severity with concrete location, impact, and fix. If no meaningful findings exist, say so and note any real validation/coverage gap.
