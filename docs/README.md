# Documentation Index

Documentation for the **ModularMonolith.Template**. Start here, then dive into the area you need.

New to the project? Read [Architecture](architecture.md) first, then [Feature Modules](modules.md) and [Adding a Module](adding-a-module.md).

## Areas

| Area | Description |
| --- | --- |
| [Architecture](architecture.md) | The modular-monolith design, the module contract and the request/message flow. |
| [API Host](api-host.md) | The thin ASP.NET Core host: wiring, security, versioning, pipeline and observability. |
| [Feature Modules](modules.md) | The standard module shape and the two example modules. |
| [Adding a Module](adding-a-module.md) | Step-by-step guide to creating a new module. |
| [Contracts](contracts.md) | The `*.Contracts` public surface: requests, responses and integration events. |
| [Shared Kernel](shared-kernel.md) | Cross-cutting primitives shared by every module and the host. |
| [Messaging & Reliability](messaging.md) | In-process messaging, outbox, inbox, idempotency and dead-lettering. |
| [Source Generators](source-generators.md) | ModularGen and MappingGen, which remove wiring boilerplate. |
| [Analysers](analysers.md) | Roslyn analysers that enforce module boundaries at build time. |
| [Testing](testing.md) | How the test suites are organised and run. |
| [Conventions](conventions.md) | The house coding style and patterns used across the codebase. |

## Diagrams

- Request & message flow — [Architecture](architecture.md)
- Outbox/inbox delivery — [Messaging & Reliability](messaging.md)
- Host startup flow — [API Host](api-host.md)
- Module ↔ contracts dependencies — [Contracts](contracts.md)

[← Back to project root](../README.md)
