# Shared Kernel

`ModularMonolith.Template.SharedKernel` holds the cross-cutting building blocks shared by every module and the host. It contains **no feature logic** — only reusable primitives and abstractions.

## Areas

- **Results** (`Results/`) — `Result` / `Result<T>` and `Error` for explicit success/failure handling instead of exceptions.
- **Validation** (`Validation/`) — FluentValidation integration and a `ValidationFilter` for endpoints.
- **Modular** (`Modular/`) — the `IModule` abstraction and module-registration helpers.
- **Messaging** (`Messaging/`) — `IMessageDispatcher`, the in-process dispatcher and handler registration extensions.
- **Outbox** (`Outbox/`) — transactional outbox processor and abstractions.
- **Inbox** (`Inbox/`) — inbox store to deduplicate inbound messages.
- **Idempotency** (`Idempotency/`) — idempotency filter/keys to make request and message processing safe to retry.
- **Persistence / Storage** (`Persistence/`, `Storage/`) — connection factories, database initialisers and SQL Server store implementations.
- **Paging** (`Paging/`) — paging service and paged-result models.
- **Auditing** (`Auditing/`) — audit-log filter capturing sensitive operations.
- **Compliance** (`Compliance/`) — attributes such as `[PersonalData]` to mark PII.
- **Security** (`Security/`) — shared security abstractions and helpers.
- **Http** (`Http/`) — HTTP helpers and constants.
- **Serialisation** (`Serialisation/`) — shared serialization configuration.
- **Configuration** (`Configuration/`) — validated options helpers (`AddValidatedOptions`).
- **Features** (`Features/`) — feature-flag abstractions.
- **Azure** (`Azure/`) — integrations such as Key Vault configuration.

## Related Docs

- [Architecture](architecture.md)
- [Messaging & Reliability](messaging.md)
- [API Host](api-host.md)
