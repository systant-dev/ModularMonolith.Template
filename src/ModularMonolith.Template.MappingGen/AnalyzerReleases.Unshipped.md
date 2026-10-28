; Unshipped analyzer release
; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
MAP001  | Mapping  | Error    | Target required member has no matching source property.
MAP002  | Mapping  | Warning  | Matching member skipped because its type is not implicitly convertible.
MAP003  | Mapping  | Warning  | Nested member skipped because a recursive mapping cycle was detected.