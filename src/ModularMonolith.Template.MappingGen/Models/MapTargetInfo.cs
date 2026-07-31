using ModularMonolith.Template.SourceGenerators.Shared.Collections;
using ModularMonolith.Template.SourceGenerators.Shared.Models;

namespace ModularMonolith.Template.MappingGen.Models;

internal readonly record struct MapTargetInfo(string Key,
                                              string TargetTypeName,
                                              EquatableArray<MapPropertyAssignment> Assignments,
                                              EquatableArray<string> UnmappedRequiredMembers,
                                              EquatableArray<MapMemberTypeMismatch> IncompatibleMembers,
                                              EquatableArray<MapCycleMember> CycleMembers,
                                              LocationInfo? Location);