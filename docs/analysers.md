# Analysers

[← Docs index](README.md)

`ModularMonolith.Template.Analysers` contains

## What They Enforce

- **Module isolation** — flags a module that references another module's implementation instead of going through its `*.Contracts` project or messaging.
- **`[Module]` correctness** — ensures `[Module]` is applied correctly (e.g. to a type implementing [`IModule`](architecture.md#the-module-contract)) so module discovery and generation work as intended.

## Why

- Keeps boundaries honest as the codebase grows and new contributors join.
- Pairs with the [source generators](source-generators.md): the generators assume valid module shapes, and the analysers guarantee them.

## Example

Referencing another module's implementation is flagged at build time; only its `*.Contracts` are allowed:

```csharp
// Flagged — reaching into AccountsModule's implementation
using AccountsModule.Services;

var service = provider.GetRequiredService<IAccountService>();

// Allowed — depend on the public contract and react via messaging
using AccountsModule.Contracts.Events;

internal sealed class AccountOpenedHandler : IMessageHandler<AccountOpenedIntegrationEvent>
{
    public Task HandleAsync(AccountOpenedIntegrationEvent message, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
```

## Related Docs

- [Architecture](architecture.md)
- [Source Generators](source-generators.md)
- [Testing](testing.md)
