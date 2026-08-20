using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;

namespace ModularMonolith.Template.SharedKernel.Features;

public sealed class FeatureFlagFilter(string featureName) : IEndpointFilter
{
    #region Public Method Declarations

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        IFeatureManager featureManager = context.HttpContext.RequestServices.GetRequiredService<IFeatureManager>();

        bool enabled = await featureManager.IsEnabledAsync(featureName);
        if (!enabled)
        {
            return TypedResults.NotFound();
        }

        return await next(context);
    }

    #endregion
}
