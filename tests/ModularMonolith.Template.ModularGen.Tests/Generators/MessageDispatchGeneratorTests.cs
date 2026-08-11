using Microsoft.CodeAnalysis.CSharp;
using ModularMonolith.Template.ModularGen.Tests.Helpers;
using ModularMonolith.Template.SourceGenerators.TestKit.Helpers;

namespace ModularMonolith.Template.ModularGen.Tests.Generators;

[Component(TestComponent.ModularGen)]
[Category(TestCategory.Generators)]
public sealed class MessageDispatchGeneratorTests
{
    #region Test Method Declarations

    [Fact]
    public Task EmitsDispatchMapFromDiscoveredHandlers()
    {
        string source = $$"""
            {{TestSources.MessagingMarkerTypes}}

            {{TestSources.ExampleMessageAndHandler}}

            namespace ModularMonolith.Template.Api.Messaging
            {
                using Microsoft.Extensions.DependencyInjection;

                public static partial class MessageDispatchServiceCollectionExtensions
                {
                    [ModularMonolith.Template.SharedKernel.Messaging.MessageDispatchMapTarget]
                    private static partial void RegisterGeneratedMessageDispatch(IServiceCollection serviceCollection);
                }
            }
            """;

        // The emitted map references System.IServiceProvider (forwarded to an assembly the synthetic
        // compilation does not reference), so run the generator without asserting output compilation
        // errors and snapshot the generated source instead.
        CSharpCompilation compilation = GeneratorTestHelper.CreateCompilation(source);
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new MessageDispatchGenerator());
        driver = driver.RunGenerators(compilation, TestContext.Current.CancellationToken);

        return Verify(driver).UseFileName("MessageDispatchGenerator");
    }

    #endregion
}
