using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace ModularMonolith.Template.SharedKernel.Http;

[SuppressMessage("Naming", "CA1708:Identifiers should differ by more than case",
    Justification = "False positive on C# extension blocks; the receiver types (IServiceCollection and IHttpClientBuilder) are distinct.")]
public static class HttpClientBuilderExtensions
{
    extension(IServiceCollection serviceCollection)
    {
        public IHttpClientBuilder AddResilientHttpClient(string name, Action<HttpClient>? configureClient = null, TimeSpan? totalRequestTimeout = null, int maxRetryAttempts = 3)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);

            IHttpClientBuilder builder = configureClient is null
                ? serviceCollection.AddHttpClient(name)
                : serviceCollection.AddHttpClient(name, configureClient);

            return builder
                .ConfigureSharedPrimaryHandler()
                .AddSharedResilienceHandler(totalRequestTimeout, maxRetryAttempts);
        }

        public IHttpClientBuilder AddResilientHttpClient<TClient>(Action<HttpClient>? configureClient = null, TimeSpan? totalRequestTimeout = null, int maxRetryAttempts = 3)
            where TClient : class
        {
            IHttpClientBuilder builder = configureClient is null
                ? serviceCollection.AddHttpClient<TClient>()
                : serviceCollection.AddHttpClient<TClient>(configureClient);

            return builder
                .ConfigureSharedPrimaryHandler()
                .AddSharedResilienceHandler(totalRequestTimeout, maxRetryAttempts);
        }
    }

    extension(IHttpClientBuilder builder)
    {
        public IHttpClientBuilder ConfigureSharedPrimaryHandler()
        {
            builder.ConfigurePrimaryHttpMessageHandler(() =>
            {
                SocketsHttpHandler handler = new()
                {
                    AutomaticDecompression = DecompressionMethods.All,
                    PooledConnectionLifetime = TimeSpan.FromMinutes(5),
                    EnableMultipleHttp2Connections = true
                };
                return handler;
            });

            return builder;
        }

        public IHttpClientBuilder AddSharedResilienceHandler(TimeSpan? totalRequestTimeout = null, int maxRetryAttempts = 3)
        {
            TimeSpan totalTimeout = totalRequestTimeout ?? TimeSpan.FromMinutes(5);
            TimeSpan attemptTimeout = TimeSpan.FromSeconds(Math.Max(1, totalTimeout.TotalSeconds / 3));

            builder.AddStandardResilienceHandler(options =>
            {
                options.TotalRequestTimeout.Timeout = totalTimeout;
                options.AttemptTimeout.Timeout = attemptTimeout;
                options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(attemptTimeout.TotalSeconds * 2);
                options.Retry.MaxRetryAttempts = maxRetryAttempts;
            });

            return builder;
        }
    }
}