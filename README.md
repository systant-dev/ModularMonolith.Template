# ModularMonolith.Template

![.NET](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet&logoColor=white)
![.NET Standard](https://img.shields.io/badge/.NET%20Standard-2.0-512BD4?logo=dotnet&logoColor=white)
![License](https://img.shields.io/badge/license-see%20LICENSE.txt-blue)

A .NET **modular monolith** template: a single deployable process composed of strictly isolated feature modules, with boundaries enforced by Roslyn analysers and boilerplate removed by custom source generators.

Targets **.NET 10** (host, modules, shared kernel, tests) and **.NET Standard 2.0** (analysers and source generators).

## Getting Started

**Prerequisites**

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server (the default configuration uses LocalDB: `(localdb)\mssqllocaldb`)

**Run the API**

```powershell
dotnet run --project src/ModularMonolith.Template.Api
```

Each module reads its own connection string from configuration under `Modules:<ModuleName>` in
[`src/ModularMonolith.Template.Api/appsettings.json`](src/ModularMonolith.Template.Api/appsettings.json):

```json
"Modules": {
  "UserModule": {
    "Enabled": true,
    "MaxUsers": 1000,
    "ConnectionString": "Server=(localdb)\\mssqllocaldb;Database=ModularMonolith;Integrated Security=true;Encrypt=False;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

Modules initialise their own databases on startup. Adjust the connection strings (and the
`Features` flags, e.g. `UserDeletion`) to suit your environment.

> **Database initialisation on startup** is environment-aware. By default it runs **only in
> Development** so local runs are friction-free, and is **disabled outside Development** so
> production migrations are applied deliberately (out-of-band). Override this with the
> `Modules:RunDatabaseInitialisationOnStartup` setting (`true`/`false`).

**Run the tests**

```powershell
dotnet test ModularMonolith.Template.slnx
```

## Docs Map

| Area | Doc |
| --- | --- |
| Architecture | [docs/architecture.md](docs/architecture.md) |
| API Host | [docs/api-host.md](docs/api-host.md) |
| Feature Modules | [docs/modules.md](docs/modules.md) |
| Adding a Module | [docs/adding-a-module.md](docs/adding-a-module.md) |
| Contracts | [docs/contracts.md](docs/contracts.md) |
| Shared Kernel | [docs/shared-kernel.md](docs/shared-kernel.md) |
| Messaging & Reliability | [docs/messaging.md](docs/messaging.md) |
| Source Generators | [docs/source-generators.md](docs/source-generators.md) |
| Analysers | [docs/analysers.md](docs/analysers.md) |
| Testing | [docs/testing.md](docs/testing.md) |
| Conventions | [docs/conventions.md](docs/conventions.md) |

## Solution Layout

```
src/
  ModularMonolith.Template.Api/            # Thin ASP.NET Core host
  ModularMonolith.Template.SharedKernel/   # Cross-cutting primitives (.NET 10)
  UserModule/  UserModule.Contracts/       # Example feature module + its public contracts
  AccountsModule/  AccountsModule.Contracts/
  ModularMonolith.Template.Analysers/          # Roslyn analysers (.NET Standard 2.0)
  ModularMonolith.Template.ModularGen/         # Source generators (.NET Standard 2.0)
  ModularMonolith.Template.MappingGen/
  ModularMonolith.Template.SourceGenerators.Shared/
tests/
  *.Tests/                                 # One suite per area, mirroring src/
docs/                                      # Documentation (see the Docs Map above)
```

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
- See [Adding a Module](docs/adding-a-module.md) for a step-by-step guide.

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
