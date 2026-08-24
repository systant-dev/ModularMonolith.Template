# API Host

`ModularMonolith.Template.Api` is the single ASP.NET Core entry point. It stays intentionally thin: it discovers modules and layers cross-cutting concerns around them.

## Responsibilities

- **Module wiring** (`Modular/`) — discovers `[Module]` types, calls `RegisterServices`, `MapEndpoints` and `RegisterHealthChecks`.
- **Security** (`Security/`) — authentication and authorization configuration.
- **Versioning** (`Versioning/`) — API versioning via `Asp.Versioning`.
- **Request pipeline** (`RequestPipeline/`) — correlation-id middleware, rate limiting and request timeouts.
- **Exception handling** (`Exceptions/`) — centralized exception handler producing consistent problem responses.
- **Health checks** (`HealthChecks/`) — aggregates the host and per-module health checks.
- **Observability** (`Observability/`) — Application Insights / telemetry wiring.
- **Logging** (`Logging/`) — structured logging configuration.
- **Messaging** (`Messaging/`) — registers the in-process message dispatcher and handlers.
- **Outbox** (`Outbox/`) — hosts the Outbox processor and dead-letter management endpoints.

## Startup Flow

1. Build configuration and logging.
2. Register shared-kernel services and cross-cutting middleware.
3. Discover and register all modules.
4. Build the app, map middleware, then map each module's endpoints.
5. Start background workers (e.g. the Outbox processor).

## Related Docs

- [Architecture](architecture.md)
- [Messaging & Reliability](messaging.md)
- [Shared Kernel](shared-kernel.md)
