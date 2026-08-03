using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModularMonolith.Template.SharedKernel.Compliance;
using System.Collections.Concurrent;
using System.Reflection;

namespace ModularMonolith.Template.SharedKernel.Tests.Compliance;

[Component(TestComponent.SharedKernel)]
[Category(TestCategory.Compliance)]
public sealed class ComplianceLoggingIntegrationTests
{
    #region Test Method Declarations

    [Fact]
    public void RedactedModel_ShouldNotLeakSensitiveDataIntoLogOutput()
    {
        const string sensitiveName = "Ada Lovelace";
        const string sensitiveEmail = "ada@example.com";

        InMemoryLoggerProvider logSink = new();

        ServiceCollection services = new();
        services.AddLogging(builder => builder.AddProvider(logSink));
        services.AddCompliance(
            options =>
            {
                options.SetRedactor(DataClassification.Personal, new ErasingRedactor());
                options.SetRedactor(DataClassification.Sensitive, new ErasingRedactor("[redacted]"));
            },
            Assembly.GetExecutingAssembly());

        using ServiceProvider provider = services.BuildServiceProvider();

        IModelRedactor<Person> redactor = provider.GetRequiredService<IModelRedactor<Person>>();
        IRedactorProvider redactorProvider = provider.GetRequiredService<IRedactorProvider>();
        ILogger<ComplianceLoggingIntegrationTests> logger =
            provider.GetRequiredService<ILogger<ComplianceLoggingIntegrationTests>>();

        Person original = new() { Name = sensitiveName, Email = sensitiveEmail };
        Person safe = redactor.Redact(original, redactorProvider);

        logger.LogInformation("Processing person {Name} with contact {Email}", safe.Name, safe.Email);

        string output = logSink.GetOutput();

        output.Should().NotContain(sensitiveName);
        output.Should().NotContain(sensitiveEmail);
        output.Should().Contain("***");
        output.Should().Contain("[redacted]");
    }

    #endregion

    #region Nested Types

    public sealed class Person
    {
        #region Property Declarations

        public string Name { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        #endregion
    }

    public sealed class PersonRedactor : IModelRedactor<Person>
    {
        #region Public Method Declarations

        public Person Redact(Person model, IRedactorProvider redactorProvider)
        {
            model.Name = redactorProvider.GetRedactor(DataClassification.Personal).Redact(model.Name);
            model.Email = redactorProvider.GetRedactor(DataClassification.Sensitive).Redact(model.Email);
            return model;
        }

        #endregion
    }

    private sealed class InMemoryLoggerProvider : ILoggerProvider
    {
        #region Field Declarations

        private readonly ConcurrentQueue<string> _messages = new();

        #endregion

        #region Public Method Declarations

        public ILogger CreateLogger(string categoryName) => new InMemoryLogger(_messages);

        public string GetOutput() => string.Join(Environment.NewLine, _messages);

        public void Dispose()
        {
        }

        #endregion

        #region Nested Types

        private sealed class InMemoryLogger(ConcurrentQueue<string> messages) : ILogger
        {
            #region Public Method Declarations

            public IDisposable? BeginScope<TState>(TState state)
                where TState : notnull => null;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(
                LogLevel logLevel,
                EventId eventId,
                TState state,
                Exception? exception,
                Func<TState, Exception?, string> formatter)
            {
                messages.Enqueue(formatter(state, exception));
            }

            #endregion
        }

        #endregion
    }

    #endregion
}
