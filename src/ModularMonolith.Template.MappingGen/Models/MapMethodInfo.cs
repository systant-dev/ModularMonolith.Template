namespace ModularMonolith.Template.MappingGen.Models;

internal readonly record struct MapMethodInfo(string Key,
                                              string Namespace,
                                              string ContainingTypeName,
                                              string ContainingTypeKeyword,
                                              string Accessibility,
                                              string MethodName,
                                              string TypeParameterName,
                                              string ConstraintClause,
                                              string SourceParameterName,
                                              string SourceTypeName);