using ModularMonolith.Template.SourceGen.Common.Collections;

namespace ModularMonolith.Template.MappingGen.Models;

internal readonly record struct MapTargetInfo(string Key,
                                              string TargetTypeName,
                                              EquatableArray<MapPropertyAssignment> Assignments,
                                              EquatableArray<string> UnmappedRequiredMembers,
                                              LocationInfo? Location);