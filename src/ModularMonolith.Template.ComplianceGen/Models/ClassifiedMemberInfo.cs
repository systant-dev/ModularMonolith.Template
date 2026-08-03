using ModularMonolith.Template.SourceGenerators.Shared.Models;

namespace ModularMonolith.Template.ComplianceGen.Models;

internal readonly record struct ClassifiedMemberInfo(string MemberPath,
                                                     string ClassificationName,
                                                     bool IsString,
                                                     bool IsSupportedSelector,
                                                     string MemberTypeName,
                                                     LocationInfo? Location);
