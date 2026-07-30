using Microsoft.Extensions.DependencyInjection;
using System.Net;

namespace ModularMonolith.Template.SharedKernel.Http;

public static class HttpClientBuilderExtensions
{
    extension(IHttpClientBuilder builder)
    {
        public IHttpClientBuilder ConfigureSharedPrimaryHandler()
        {
            builder.ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
            {
                AutomaticDecompression = DecompressionMethods.All,
                PooledConnectionLifetime = TimeSpan.FromMinutes(5),
                EnableMultipleHttp2Connections = true
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