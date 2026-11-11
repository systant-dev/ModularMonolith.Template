namespace ModularMonolith.Template.MappingGen.Models;

internal readonly record struct MapMemberTypeMismatch(string PropertyName, string SourceTypeName, string TargetTypeName);
