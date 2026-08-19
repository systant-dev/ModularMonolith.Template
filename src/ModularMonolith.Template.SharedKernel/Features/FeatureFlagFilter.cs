using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ModularMonolith.Template.SharedKernel.Features;

public sealed class FeatureFlagFilter(string featureName) : IEndpointFilter
{
    #region Field Declarations

    public const string ConfigurationSectionName = "Features";

    #endregion

    #region Public Method Declarations

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        IConfiguration configuration = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();

        bool enabled = configuration.GetValue($"{ConfigurationSectionName}:{featureName}", defaultValue: false);
        if (!enabled)
        {
            return TypedResults.NotFound();
        }

        return await next(context);
    }

    #endregion
}
