using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Template.SharedKernel.Results;
using ModularMonolith.Template.SharedKernel.Rules;

namespace ModularMonolith.Template.SharedKernel.Tests.Rules;

[Component(TestComponent.SharedKernel)]
[Category(TestCategory.Rules)]
public sealed class RulesServiceCollectionExtensionsTests
{
    #region Test Method Declarations

    [Fact]
    public void AddRuleProcessors_RegistersGenericProcessorAsScoped()
    {
        ServiceCollection services = new();

        services.AddRuleProcessors();

        services.Should().ContainSingle(descriptor => descriptor.ServiceType == typeof(IRuleProcessor<>) &&
                                                      descriptor.ImplementationType == typeof(RuleProcessor<>) &&
                                                      descriptor.Lifetime == ServiceLifetime.Scoped);
    }

    [Fact]
    public void AddRule_WithExplicitRule_RegistersRuleAsScoped()
    {
        ServiceCollection services = new();

        services.AddRule<TestContext, ExplicitRule>();

        using ServiceProvider provider = services.BuildServiceProvider();
        using IServiceScope scope = provider.CreateScope();

        IRule<TestContext> rule = scope.ServiceProvider.GetRequiredService<IRule<TestContext>>();

        rule.Should().BeOfType<ExplicitRule>();
        services.Should().ContainSingle(descriptor => descriptor.ServiceType == typeof(IRule<TestContext>) &&
                                                      descriptor.ImplementationType == typeof(ExplicitRule) &&
                                                      descriptor.Lifetime == ServiceLifetime.Scoped);
    }

    [Fact]
    public void AddRule_RegistersRulesFromCallingAssemblyAsScoped()
    {
        ServiceCollection services = new();

        services.AddRule<AutoDiscoveryContext>();

        using ServiceProvider provider = services.BuildServiceProvider();
        using IServiceScope scope = provider.CreateScope();

        IEnumerable<IRule<AutoDiscoveryContext>> rules = scope.ServiceProvider.GetServices<IRule<AutoDiscoveryContext>>();

        rules.Should().ContainSingle().Which.Should().BeOfType<AutoDiscoveredRule>();
        services.Should().Contain(descriptor => descriptor.ServiceType == typeof(IRule<AutoDiscoveryContext>) &&
                                                descriptor.ImplementationType == typeof(AutoDiscoveredRule) &&
                                                descriptor.Lifetime == ServiceLifetime.Scoped);
    }

    #endregion

    #region Nested Types

    private sealed record TestContext;

    private sealed class ExplicitRule : IRule<TestContext>
    {
        public int Order => 1;

        public bool CanExecute(TestContext context) => true;

        public Task<Result> ExecuteAsync( TestContext context, CancellationToken cancellationToken) => Task.FromResult(Result.Success());
    }

    private sealed record AutoDiscoveryContext;

    private sealed class AutoDiscoveredRule : IRule<AutoDiscoveryContext>
    {
        public int Order => 1;

        public bool CanExecute(AutoDiscoveryContext context) => true;

        public Task<Result> ExecuteAsync( AutoDiscoveryContext context,CancellationToken cancellationToken) => Task.FromResult(Result.Success());
    }

    #endregion
}
