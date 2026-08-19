using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace ModularMonolith.Template.SharedKernel.Features;

public static class FeatureFlagEndpointExtensions
{
    extension<TBuilder>(TBuilder builder) where TBuilder : IEndpointConventionBuilder
    {
        public TBuilder RequireFeature(string featureName)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(featureName);
            FeatureFlagFilter filter = new(featureName);
            builder.AddEndpointFilter(filter);
            return builder;
        }
    }
}
