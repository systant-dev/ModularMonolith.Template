using System.Collections.Immutable;

namespace ModularMonolith.Template.MappingGen.Models;

internal readonly record struct MapTargetInfo(string Key, string TargetTypeName, ImmutableArray<MapPropertyAssignment> Assignments);