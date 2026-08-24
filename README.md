# ModularMonolith.Template

A .NET **modular monolith** template: a single deployable process composed of strictly isolated feature modules, with boundaries enforced by Roslyn analysers and boilerplate removed by custom source generators.

Targets **.NET 10** (host, modules, shared kernel, tests) and **.NET Standard 2.0** (analysers and source generators).

## Functionality Areas

Each area below has a dedicated doc in the [`docs/`](docs) folder.

### Architecture — [docs/architecture.md](docs/architecture.md)
- Single deployable process made of independently-designed, self-contained feature modules.
- Modules never reference each other directly — only via `*.Contracts` and in-process messaging.
- The host is thin; boundaries are enforced by tooling (analysers) and wiring is generated (source generators).
- Every module implements `IModule` and is marked with `[Module]`.

### API Host — [docs/api-host.md](docs/api-host.md)
- `ModularMonolith.Template.Api` is the single ASP.NET Core entry point, kept intentionally thin.
- Handles module wiring, security, API versioning, request pipeline and centralized exception handling.
- Aggregates host and per-module health checks, observability, logging and messaging registration.

### Feature Modules — [docs/modules.md](docs/modules.md)
- Ships two example modules — **UserModule** and **AccountsModule** — as self-contained vertical slices.
- Standard shape: `Module.cs`, `Endpoints/`, `Services/`, `Storage/`, `Mappings/`, `Validation/`, `Messaging/`, `Models/`, `Configuration/`.
- Each module owns its endpoints, domain logic, persistence and message handlers.

### Contracts — [docs/contracts.md](docs/contracts.md)
- `*.Contracts` projects define the **public surface** of each module — the only types others may depend on.
- Organized into `Requests/`, `Responses/` and `Events/` (integration events).
- Keeps modules isolated and stable; enforced by the analysers.

### Shared Kernel — [docs/shared-kernel.md](docs/shared-kernel.md)
- `ModularMonolith.Template.SharedKernel` holds reusable cross-cutting primitives — no feature logic.
- Includes `Result`/`Result<T>`, validation, the `IModule` abstraction, messaging, outbox/inbox, idempotency, persistence and paging.

### Messaging & Reliability — [docs/messaging.md](docs/messaging.md)
- In-process message dispatch (`IMessageDispatcher`) routes messages to registered handlers.
- Transactional **outbox** guarantees events survive crashes; **dead-letter** isolates repeatedly-failing messages.
- **Inbox** and **idempotency** make processing safe under retries.

### Source Generators — [docs/source-generators.md](docs/source-generators.md)
- **ModularGen** generates module registration/wiring from `[Module]` types.
- **MappingGen** generates object-to-object mapping code (domain models ↔ contract DTOs).
- **SourceGenerators.Shared** provides shared incremental-generator infrastructure.

### Analysers — [docs/analysers.md](docs/analysers.md)
- Roslyn analysers turn architecture violations into build errors instead of review comments.
- Enforce **module isolation** (no cross-module implementation references) and correct `[Module]` usage.
- Pair with the source generators, which assume valid module shapes.

### Testing — [docs/testing.md](docs/testing.md)
- The `tests/` folder mirrors `src/`, with a suite per area.
- Covers host/integration, per-module, shared-kernel, generator and analyser tests.
- Run everything with `dotnet test ModularMonolith.Template.slnx`.
