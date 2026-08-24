# Source Generators

Custom Roslyn source generators remove repetitive wiring at compile time. They target **.NET Standard 2.0** (a Roslyn requirement) and share infrastructure.

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

## Related Docs

- [Architecture](architecture.md)
- [Analysers](analysers.md)
- [Testing](testing.md)
