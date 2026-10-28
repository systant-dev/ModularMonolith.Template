# Source Generators

[← Docs index](README.md)

Custom Roslyn source generators

## Generators

- **ModularGen** (`ModularMonolith.Template.ModularGen`) — discovers `[Module]` types and generates their registration/wiring code so the host does not need manual module lists.
- **MappingGen** (`ModularMonolith.Template.MappingGen`) — generates object-to-object mapping code (e.g. domain models ↔ contract DTOs), removing hand-written mapping boilerplate.
- **SourceGenerators.Shared** (`ModularMonolith.Template.SourceGenerators.Shared`) — shared generator infrastructure: syntax/semantic analysis helpers, collection utilities, language-feature helpers and models used by both generators.

## Internal Structure (per generator)

- `Analysis/` — inspect the compilation and gather symbols.
- `Models/` — immutable models describing what to emit (incremental-generator friendly).
- `Emit/` — build the generated source text.
- `Generators/` — the `IIncrementalGenerator` entry points.
- `Diagnostics/` — generator-specific diagnostics.
- `LanguageFeatures/` — polyfills/helpers for targeting older TFMs.

## Example

**MappingGen** — declare a `partial` mapping method marked with `[Map]`; the generator emits the body:

```csharp
internal static partial class UserMappings
{
    [Map]
    public static partial UserResponse ToResponse(this User source);
}

// usage
UserResponse response = user.ToResponse();
```

**ModularGen** — marking a type with `[Module]` lets the generator discover and wire it, so the host never needs a hand-maintained module list:

```csharp
[Module]
public sealed class Module : IModule
{
    public void RegisterServices(IServiceCollection serviceCollection) { /* ... */ }
    public void MapEndpoints(IEndpointRouteBuilder endpoints) { /* ... */ }
}
```

## Related Docs

- [Architecture](architecture.md)
- [Analysers](analysers.md)
- [Testing](testing.md)
