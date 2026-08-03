using Microsoft.CodeAnalysis;
using ModularMonolith.Template.ComplianceGen.Analysis;
using ModularMonolith.Template.ComplianceGen.Diagnostics;
using ModularMonolith.Template.ComplianceGen.Emit;
using ModularMonolith.Template.ComplianceGen.Models;
using System.Collections.Immutable;

namespace ModularMonolith.Template.ComplianceGen.Generators;

[Generator]
public sealed class ComplianceGenerator : IIncrementalGenerator
{
    #region Field Declarations

    private const string ClassificationProfileAttributeName = "ModularMonolith.Template.SharedKernel.Compliance.ClassificationProfileAttribute";

    #endregion

    #region Public Method Declarations

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValueProvider<ImmutableArray<ClassificationProfileInfo>> profileProvider = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                ClassificationProfileAttributeName,
                predicate: static (node, _) => ClassificationProfileAnalyser.IsCandidateProfile(node),
                transform: static (attributeContext, cancellationToken) => ClassificationProfileAnalyser.GetProfileInfo(attributeContext, cancellationToken))
            .Where(static profile => profile is not null)
            .Select(static (profile, _) => profile!.Value)
            .Collect()
            .WithTrackingName("ClassificationProfiles");

        context.RegisterSourceOutput(profileProvider, static (sourceContext, profiles) =>
        {
            if (profiles.IsDefaultOrEmpty)
            {
                return;
            }

            foreach (ClassificationProfileInfo profile in profiles)
            {
                foreach (Diagnostic diagnostic in ComplianceDiagnostics.GetDiagnostics(profile))
                {
                    sourceContext.ReportDiagnostic(diagnostic);
                }

                string source = RedactorEmitter.Emit(profile);
                sourceContext.AddSource($"{profile.Key}.g.cs", source);
            }
        });
    }

    #endregion
}