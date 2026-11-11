using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Testing;
using ModularMonolith.Template.SharedKernel.Results;
using ModularMonolith.Template.SharedKernel.Rules;

namespace ModularMonolith.Template.SharedKernel.Tests.Rules;

[Component(TestComponent.SharedKernel)]
[Category(TestCategory.Rules)]
public sealed class RuleProcessorTests
{
    #region Test Method Declarations

    [Fact]
    public async Task RunRulesAsync_ExecutesApplicableRulesInOrder()
    {
        List<string> executionOrder = [];
        TestRule secondRule = new(20, "second", executionOrder);
        TestRule firstRule = new(10, "first", executionOrder);
        RuleProcessor<TestRuleContext> processor = CreateProcessor(secondRule, firstRule);
        TestRuleContext context = new();

        Result result = await processor.RunRulesAsync(context, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        executionOrder.Should().Equal("first", "second");
    }

    [Fact]
    public async Task RunRulesAsync_WhenRuleFails_ReturnsFailureAndStopsProcessing()
    {
        List<string> executionOrder = [];
        Error error = Error.Validation("rule.failed", "The rule failed.");
        TestRule failedRule = new(10, "failed", executionOrder, error);
        TestRule notExecutedRule = new(20, "not-executed", executionOrder);
        RuleProcessor<TestRuleContext> processor = CreateProcessor(failedRule, notExecutedRule);
        TestRuleContext context = new();

        Result result = await processor.RunRulesAsync(context, TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
        executionOrder.Should().Equal("failed");
    }

    [Fact]
    public async Task RunRulesAsync_WhenRuleCannotExecute_SkipsRule()
    {
        List<string> executionOrder = [];
        TestRule skippedRule = new(10, "skipped", executionOrder, canExecute: false);
        TestRule executedRule = new(20, "executed", executionOrder);
        RuleProcessor<TestRuleContext> processor = CreateProcessor(skippedRule, executedRule);
        TestRuleContext context = new();

        Result result = await processor.RunRulesAsync(context, TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        executionOrder.Should().Equal("executed");
    }

    [Fact]
    public async Task RunRulesAsync_WhenCancellationIsRequested_ThrowsOperationCanceledException()
    {
        using CancellationTokenSource cancellationTokenSource = new();
        cancellationTokenSource.Cancel();
        List<string> executionOrder = [];
        TestRule rule = new(10, "rule", executionOrder);
        RuleProcessor<TestRuleContext> processor = CreateProcessor(rule);
        TestRuleContext context = new();

        Func<Task> act = () => processor.RunRulesAsync(context, cancellationTokenSource.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task RunRulesAsync_WhenRuleFails_LogsTheRuleAndError()
    {
        Error error = Error.Validation("rule.failed", "The rule failed.");
        FakeLogger<RuleProcessor<TestRuleContext>> logger = new();
        List<string> executionOrder = [];
        TestRule failedRule = new(10, "failed", executionOrder, error);
        IRule<TestRuleContext>[] rules = [failedRule];
        RuleProcessor<TestRuleContext> processor = new(rules, logger);
        TestRuleContext context = new();

        await processor.RunRulesAsync(context, TestContext.Current.CancellationToken);

        logger.LatestRecord.Message.Should().Contain("failed").And.Contain("rule.failed");
    }

    #endregion

    #region Private Method Declarations

    private static RuleProcessor<TestRuleContext> CreateProcessor(params IRule<TestRuleContext>[] rules) =>
        new(rules, NullLogger<RuleProcessor<TestRuleContext>>.Instance);

    #endregion

    #region Nested Types

    private sealed record TestRuleContext;

    private sealed class TestRule(int order, string name, List<string> executionOrder, Error? error = null, bool canExecute = true) : IRule<TestRuleContext>
    {
        public int Order => order;

        public bool CanExecute(TestRuleContext context) => canExecute;

        public Task<Result> ExecuteAsync(TestRuleContext context, CancellationToken cancellationToken)
        {
            executionOrder.Add(name);
            return Task.FromResult(error is null ? Result.Success() : Result.Failure(error));
        }
    }

    #endregion
}
