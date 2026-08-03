using Microsoft.CodeAnalysis;
using ModularMonolith.Template.ComplianceGen.Models;
using System.Collections.Generic;

namespace ModularMonolith.Template.ComplianceGen.Diagnostics;

internal static class ComplianceDiagnostics
{
    #region Field Declarations

    public static readonly DiagnosticDescriptor UnsupportedSelector = new(
        id: "CMP001",
        title: "Unsupported classification selector",
        messageFormat: "The Classify selector for profile '{0}' must be a simple or nested member access expression (for example x => x.Email)",
        category: "Compliance",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "The source generator reads the Classify selector as syntax; only member access chains such as x => x.Email or x => x.Address.PostCode are supported.");

    public static readonly DiagnosticDescriptor NonStringMember = new(
        id: "CMP002",
        title: "Classified member is not a string",
        messageFormat: "Member '{0}' on profile '{1}' cannot be redacted because its type '{2}' is not string",
        category: "Compliance",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Redactors operate on string values; classify a string member or convert the value before redaction.");

    #endregion

    #region Public Method Declarations

    public static IEnumerable<Diagnostic> GetDiagnostics(ClassificationProfileInfo profile)
    {
        foreach (ClassifiedMemberInfo member in profile.Members)
        {
            Location? location = member.Location?.ToLocation();

            if (!member.IsSupportedSelector)
            {
                yield return Diagnostic.Create(
                    UnsupportedSelector,
                    location,
                    profile.ProfileTypeName);
                continue;
            }

            if (!member.IsString)
            {
                yield return Diagnostic.Create(
                    NonStringMember,
                    location,
                    member.MemberPath,
                    profile.ProfileTypeName,
                    member.MemberTypeName);
            }
        }
    }

    #endregion
}
