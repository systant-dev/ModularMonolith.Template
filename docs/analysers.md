# Analysers

`ModularMonolith.Template.Analysers` contains Roslyn analysers that enforce the modular-monolith rules at compile time, so architecture violations become build errors rather than review comments. They target **.NET Standard 2.0**.

## What They Enforce

- **Module isolation** — flags a module that references another module's implementation instead of going through its `*.Contracts` project or messaging.
- **`[Module]` correctness** — ensures `[Module]` is applied correctly (e.g. to a type implementing `IModule`) so module discovery and generation work as intended.

## Why

- Keeps boundaries honest as the codebase grows and new contributors join.
- Pairs with the [source generators](source-generators.md): the generators assume valid module shapes, and the analysers guarantee them.

## Related Docs

- [Architecture](architecture.md)
- [Source Generators](source-generators.md)
- [Testing](testing.md)
