# Feature Modules

The template ships two example modules — **UserModule** and **AccountsModule** — that demonstrate the standard module shape. Each is a self-contained vertical slice.

## Standard Module Shape

- **`Module.cs`** — the `[Module]` entry point implementing `IModule` (`RegisterServices`, `MapEndpoints`, optional `RegisterHealthChecks`).
- **`Endpoints/`** — versioned minimal-API endpoints.
- **`Services/`** — domain/application services returning `Result`/`Result<T>`.
- **`Storage/`** — module-owned persistence (connection factory, stores, database initialiser).
- **`Mappings/`** — object mappings (assisted by the mapping generator).
- **`Validation/`** — FluentValidation validators for incoming requests.
- **`Messaging/`** — handlers for integration events from other modules.
- **`Models/`** — internal domain models (never exposed across boundaries).
- **`Configuration/`** — strongly-typed, validated module options.
- **`Logging/`** — module-specific logging helpers.

## UserModule

Manages users. Registers user storage/services and validators, maps user endpoints, publishes `UserUpdatedIntegrationEvent` / `UserDeletedIntegrationEvent`, and reacts to `AccountOpenedIntegrationEvent` via `AccountOpenedHandler`. Includes `Migrations/`.

## AccountsModule

Manages accounts with its own endpoints, services, storage, mappings, messaging and validation, publishing account-related integration events.

## Rules

- A module may reference `SharedKernel` and other modules' **Contracts** — never another module's implementation.
- Cross-module communication happens only through contracts and messaging. This is enforced by the [analysers](analysers.md).

## Related Docs

- [Architecture](architecture.md)
- [Contracts](contracts.md)
- [Messaging & Reliability](messaging.md)
