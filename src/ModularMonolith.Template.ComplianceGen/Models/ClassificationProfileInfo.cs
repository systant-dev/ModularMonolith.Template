using ModularMonolith.Template.SourceGenerators.Shared.Collections;

namespace ModularMonolith.Template.ComplianceGen.Models;

internal readonly record struct ClassificationProfileInfo(string Key,
                                                          string Namespace,
                                                          string ProfileTypeName,
                                                          string ProfileTypeKeyword,
                                                          string Accessibility,
                                                          string ModelTypeName,
                                                          EquatableArray<ClassifiedMemberInfo> Members);
