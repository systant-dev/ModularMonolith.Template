using ModularMonolith.Template.MappingGen.Models;
using System.Collections.Immutable;

namespace ModularMonolith.Template.MappingGen.Analysis;

/// <summary>
/// The outcome of resolving how a source type maps onto a target type. This is a transient
/// value consumed immediately when building <see cref="MapTargetInfo"/>, so it intentionally
/// uses <see cref="ImmutableArray{T}"/> rather than the cache-equatable collections.
/// </summary>
internal readonly record struct MappingResolution(
    ImmutableArray<MapPropertyAssignment> Assignments,
    ImmutableArray<string> UnmappedRequiredMembers,
    ImmutableArray<MapMemberTypeMismatch> IncompatibleMembers,
    ImmutableArray<MapCycleMember> CycleMembers);
