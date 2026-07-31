using ModularMonolith.Template.SourceGen.Common.Collections;
using ModularMonolith.Template.SourceGen.Common.Models;

namespace ModularMonolith.Template.MappingGen.Models;

internal readonly record struct MapTargetInfo(string Key,
                                              string TargetTypeName,
                                              EquatableArray<MapPropertyAssignment> Assignments,
                                              EquatableArray<string> UnmappedRequiredMembers,
                                              LocationInfo? Location);