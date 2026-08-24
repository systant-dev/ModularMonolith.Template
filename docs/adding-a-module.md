# Adding a Module

[← Docs index](README.md)

This guide walks through

Throughout, replace `Widget` with your module's name.

## 1. Create the projects

Add two projects under `src/`:

- `WidgetModule` — the module implementation (targets **.NET 10**).
- `WidgetModule.Contracts` — the module's public surface (requests, responses, integration events).

Register both in the solution's `src` folder (the Example Modules follow the same layout).

## 2. Define the contracts

Keep contract types small, serializable and free of behaviour. Integration events derive from `Message`.

```csharp
// WidgetModule.Contracts/Requests/CreateWidgetRequest.cs
public sealed record CreateWidgetRequest
{
	public required string Name { get; init; }
}

// WidgetModule.Contracts/Events/WidgetCreatedIntegrationEvent.cs
public sealed record WidgetCreatedIntegrationEvent : Message
{
	public required int Id { get; init; }
	public required string Name { get; init; }
}
```

See [Contracts](contracts.md) for the full guidelines.

## 3. Add the module entry point

Implement [`IModule`](architecture.md#the-module-contract) and mark the class with `[Module]` so the generator discovers it — no manual registration lists needed.

```csharp
[Module]
public sealed class Module : IModule
{
	public void RegisterServices(IServiceCollection serviceCollection)
	{
		serviceCollection.AddValidatedOptions<WidgetModuleOptions, WidgetModuleOptionsValidator>(nameof(WidgetModule));

		serviceCollection.AddScoped<IWidgetService, WidgetService>();
		serviceCollection.AddScoped<IValidator<CreateWidgetRequest>, CreateWidgetRequestValidator>();

		serviceCollection.AddMessageContract<WidgetCreatedIntegrationEvent>();
	}

	public void MapEndpoints(IEndpointRouteBuilder endpoints)
	{
		ApiVersion apiVersion = new(1, 0);
		ApiVersionSet apiVersionSet = endpoints.NewApiVersionSet()
											   .HasApiVersion(apiVersion)
											   .ReportApiVersions()
											   .Build();

		RouteGroupBuilder group = endpoints.MapGroup("/v{version:apiVersion}/widgets")
										   .WithApiVersionSet(apiVersionSet)
										   .HasApiVersion(apiVersion);

		group.MapPost("/", WidgetEndpoints.Create)
			 .AddEndpointFilter<ValidationFilter<CreateWidgetRequest>>();
	}
}
```

## 4. Fill in the standard module shape

Add the usual folders (see [Feature Modules](modules.md)):

- `Endpoints/` — minimal-API handlers returning `Result`/`Result<T>`.
- `Services/` — domain/application services (return `Result` instead of throwing for expected failures).
- `Storage/` — module-owned persistence and an `IModuleDatabaseInitialiser`.
- `Mappings/` — `partial` methods marked `[Map]` for the [mapping generator](source-generators.md).
- `Validation/` — FluentValidation validators.
- `Messaging/` — `IMessageHandler<TMessage>` implementations for events you subscribe to.
- `Models/` — internal domain models (never exposed across boundaries).
- `Configuration/` — a validated `ModuleOptions` record.

## 5. Wire configuration

Add a section under `Modules:WidgetModule` in
[`appsettings.json`](../src/ModularMonolith.Template.Api/appsettings.json):

```json
"Modules": {
  "WidgetModule": {
	"Enabled": true,
	"ConnectionString": "Server=(localdb)\\mssqllocaldb;Database=ModularMonolithWidgets;Integrated Security=true;Encrypt=False;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

## 6. Communicate across boundaries

- To **publish** an event: register it with `AddMessageContract<T>()` and enqueue it transactionally via the outbox (see [Messaging & Reliability](messaging.md)).
- To **react** to another module's event: reference only that module's `*.Contracts` and register a handler with `AddMessageHandler<TEvent, THandler>()`.

> Referencing another module's implementation is a build error, caught by the [analysers](analysers.md).

## 7. Add tests

Mirror the module under `tests/` (e.g. `ModularMonolith.Template.WidgetModule.Tests`) with unit tests for services, validation and handlers. See [Testing](testing.md).

## Related Docs

- [Feature Modules](modules.md)
- [Contracts](contracts.md)
- [Messaging & Reliability](messaging.md)
- [Source Generators](source-generators.md)
- [Analysers](analysers.md)
