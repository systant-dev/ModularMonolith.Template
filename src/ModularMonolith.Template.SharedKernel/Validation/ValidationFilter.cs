using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ModularMonolith.Template.SharedKernel.Validation;

public sealed class ValidationFilter<TRequest>(IValidator<TRequest> validator, ILogger<ValidationFilter<TRequest>> logger) : IEndpointFilter
    where TRequest : class
{
    #region Public Method Declarations

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        TRequest? request = context.Arguments.OfType<TRequest>().FirstOrDefault();

        if (request is not null)
        {
            ValidationResult validation = await validator.ValidateAsync(request, context.HttpContext.RequestAborted);
            if (!validation.IsValid)
            {
                logger.LogWarning("Validation failed for {RequestType}", typeof(TRequest).Name);
                return TypedResults.ValidationProblem(validation.ToDictionary());
            }
        }

        return await next(context);
    }

    #endregion
}
