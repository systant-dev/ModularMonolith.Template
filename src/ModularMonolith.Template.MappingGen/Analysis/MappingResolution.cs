using ModularMonolith.Template.MappingGen.Models;
using System.Collections.Immutable;

namespace ModularMonolith.Template.MappingGen.Analysis;

internal readonly record struct MappingResolution(ImmutableArray<MapPropertyAssignment> Assignments,
                                                  ImmutableArray<string> UnmappedRequiredMembers,
                                                  ImmutableArray<MapMemberTypeMismatch> IncompatibleMembers,
                                                  ImmutableArray<MapCycleMember> CycleMembers);
