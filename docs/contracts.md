# Contracts

The `*.Contracts` projects (`UserModule.Contracts`, `AccountsModule.Contracts`) define the **public surface** of each module. They are the only types other modules and the host are allowed to depend on.

## Structure

- **`Requests/`** — input DTOs for endpoints (e.g. `CreateUserRequest`, `UpdateUserRequest`).
- **`Responses/`** — output DTOs returned to callers (e.g. `UserResponse`).
- **`Events/`** — integration events published on the message bus (e.g. `UserCreatedIntegrationEvent`, `AccountOpenedIntegrationEvent`).

## Why Contracts Are Separate

- **Isolation.** A module can consume another module's events/requests without touching its implementation.
- **Stability.** Contracts change deliberately; internal models can evolve freely behind them.
- **Enforceability.** The [analysers](analysers.md) allow references to `*.Contracts` but flag references to module implementations.

## Guidelines

- Keep contract types small, serializable and free of behaviour.
- Version integration events carefully — other modules depend on their shape.
- Never leak internal domain models through contracts.

## Related Docs

- [Feature Modules](modules.md)
- [Messaging & Reliability](messaging.md)
