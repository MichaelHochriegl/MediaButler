# Domain unit tests

## Location

Existing project: `tests/Domain.Tests.Unit`.

Mirror production domain folders where useful.

## Stack

Use the existing xUnit v3 / Microsoft Testing Platform + AwesomeAssertions stack.

## What belongs here

- value-object validation/normalization
- aggregate/entity invariants
- allowed/forbidden state transitions
- path/source/media-kind rules
- deterministic domain calculations

Keep these tests fast and infrastructure-free.

## Style

- Construct real domain objects directly.
- Avoid mocks for domain objects.
- Follow nearby naming, currently including `Given_..._Should_...` patterns.
- Assert meaningful behavior/invariants, not private implementation.
- Cover boundary/invalid cases that define the domain contract.

Do not move transport-only validation here.
