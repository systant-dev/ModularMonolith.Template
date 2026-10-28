using FluentAssertions;
using Microsoft.Extensions.Compliance.Classification;
using Microsoft.Extensions.Compliance.Redaction;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using ModularMonolith.Template.SharedKernel.Compliance;

namespace ModularMonolith.Template.SharedKernel.Tests.Compliance;

[Component(TestComponent.SharedKernel)]
[Category(TestCategory.Compliance)]
public sealed class ComplianceRedactionIntegrationTests
{
    #region Test Method Declarations

    [Fact]
    public void LogProperties_MasksPersonalData_ErasesSensitiveData_AndPreservesUnclassified()
    {
        using ServiceProvider provider = BuildProvider();
        ILogger logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger("Test");

        RedactionSubjectLog.SubjectCreated(logger, new RedactionSubject
        {
            Id = 42,
            FullName = "Jane Doe",
            Email = "jane@example.com",
        });

        FakeLogCollector collector = provider.GetRequiredService<FakeLogCollector>();
        IReadOnlyList<KeyValuePair<string, string?>> state = collector.LatestRecord.StructuredState!;

        string? fullName = state.Single(entry => entry.Key.EndsWith("FullName", StringComparison.Ordinal)).Value;
        fullName.Should().NotBeNullOrEmpty();
        fullName.Should().MatchRegex("^\\*+$").And.NotContain("Jane");

        state.Should().ContainSingle(entry => entry.Key.EndsWith("Email")).Which.Value.Should().BeEmpty();
        state.Should().ContainSingle(entry => entry.Key.EndsWith("Id")).Which.Value.Should().Be("42");
    }

    #endregion

    #region Private Method Declarations

    private static ServiceProvider BuildProvider()
    {
        ServiceCollection services = new();

        services.AddLogging(logging =>
        {
            logging.AddFakeLogging();
            logging.EnableRedaction();
        });

        services.AddRedaction(redaction =>
        {
            DataClassificationSet personalDataClassificationSet = new(DataTaxonomy.PersonalData);
            DataClassificationSet sensitiveDataClassificationSet = new(DataTaxonomy.SensitiveData);
            redaction.SetRedactor<MaskingRedactor>(personalDataClassificationSet);
            redaction.SetRedactor<ErasingRedactor>(sensitiveDataClassificationSet);
        });

        return services.BuildServiceProvider();
    }

    #endregion
}

internal sealed record RedactionSubject
{
    #region Property Declarations

    public required int Id { get; init; }

    [PersonalData]
    public required string FullName { get; init; }

    [SensitiveData]
    public required string Email { get; init; }

    #endregion
}

internal static partial class RedactionSubjectLog
{
    #region Log Message Declarations

    [LoggerMessage(LogLevel.Information, "Subject created")]
    public static partial void SubjectCreated(ILogger logger, [LogProperties] RedactionSubject subject);

    #endregion
}
