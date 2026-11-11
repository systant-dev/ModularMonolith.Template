# Conventions

The house style used across the codebase. Following these keeps modules consistent and keeps the [analysers](analysers.md) happy.

## Boundaries

- **Modules never reference another module's implementation.** Depend only on its `*.Contracts` project or communicate via [messaging](messaging.md). This is enforced by the [analysers](analysers.md).
- **Contracts are behaviour-free.** Requests, responses and integration events are small, serializable records. Never leak internal domain models across a boundary. See [Contracts](contracts.md).
- Every module is a single `[Module]` type implementing [`IModule`](architecture.md#the-module-contract).

## Types & accessibility

- Prefer `internal sealed` for implementation types (services, handlers, validators, stores, endpoints). Only contract types and shared-kernel abstractions are `public`.
- Use `sealed record` for DTOs and messages; use `required` init-only properties for mandatory values.
- Use primary constructors for dependency injection (e.g. `internal sealed class UserService(IUserStore store, ILogger<UserService> logger)`).

## Errors & results

- Return `Result` / `Result<T>` for **expected** failures instead of throwing. Reserve exceptions for truly exceptional/programmer errors.
- Build failures with the `Error` factory methods (`Error.NotFound`, `Error.Conflict`, etc.) and let the implicit conversion produce the `Result<T>`.
- At the endpoint boundary, map failures to HTTP with `result.Error.ToProblem()`. See the [Error catalogue](shared-kernel.md#error-catalogue).

## Endpoints

- Use versioned minimal APIs under `/v{version:apiVersion}/...`. See [API versioning](api-host.md#api-versioning).
- Keep endpoint handlers thin: validate via `ValidationFilter<T>`, delegate to a service, translate the `Result` to a typed result.
- Compose cross-cutting behaviour with endpoint filters (validation, idempotency, auditing, authorisation).

## Validation

- Use FluentValidation validators (`AbstractValidator<T>`), registered per request type, and applied through `ValidationFilter<T>` rather than inline checks.

## Mapping

- Use the [mapping generator](source-generators.md): declare `partial` methods marked `[Map]`; do not hand-write mapping code.

## Formatting

- Files use `#region` blocks to group members (`Property Declarations`, `Public Method Declarations`, etc.) — follow the surrounding pattern when editing.
- Respect the repository [`.editorconfig`](../.editorconfig) for indentation, naming and analyzer severities.

## Testing

- Mirror `src/` under `tests/`, one suite per area. Unit-test services with in-memory stores. See [Testing](testing.md).

## Related Docs

- [Architecture](architecture.md)
- [Feature Modules](modules.md)
- [Adding a Module](adding-a-module.md)

[← Docs index](README.md)
