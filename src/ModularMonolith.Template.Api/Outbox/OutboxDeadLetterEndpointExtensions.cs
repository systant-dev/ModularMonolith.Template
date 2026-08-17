using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Options;
using ModularMonolith.Template.SharedKernel.Outbox;
using ModularMonolith.Template.SharedKernel.Security;

namespace ModularMonolith.Template.Api.Outbox;

public static class OutboxDeadLetterEndpointExtensions
{
    extension(IEndpointRouteBuilder endpoints)
    {
        public IEndpointRouteBuilder MapOutboxDeadLetterEndpoints()
        {
            endpoints.MapGet("/admin/outbox/dead-letters", GetDeadLettersAsync)
                     .RequireAuthorization(AuthorisationPolicies.ReadAccess)
                     .WithName("GetOutboxDeadLetters");

            return endpoints;
        }
    }

    #region Private Method Declarations

    private static async Task<Ok<IReadOnlyList<OutboxDeadLetterResponse>>> GetDeadLettersAsync(IServiceScopeFactory scopeFactory,
                                                                                               OutboxStoreRegistry registry,
                                                                                               IOptions<OutboxOptions> options,
                                                                                               int? batchSize,
                                                                                               CancellationToken cancellationToken)
    {
        OutboxOptions outboxOptions = options.Value;
        int limit = batchSize is > 0 ? batchSize.Value : outboxOptions.BatchSize;

        List<OutboxDeadLetterResponse> deadLetters = [];

        foreach (Type storeType in registry.StoreTypes)
        {
            await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();

            if (scope.ServiceProvider.GetRequiredService(storeType) is not IOutboxStore store)
            {
                continue;
            }

            IReadOnlyList<OutboxMessage> messages = await store
                .GetDeadLetteredAsync(outboxOptions.MaxAttempts, limit, cancellationToken);

            deadLetters.AddRange(messages.Select(message => new OutboxDeadLetterResponse(
                message.Id,
                message.Type,
                message.OccurredOnUtc,
                message.Attempts,
                message.Error)));
        }

        IReadOnlyList<OutboxDeadLetterResponse> response = [.. deadLetters.OrderBy(deadLetter => deadLetter.OccurredOnUtc)];
        return TypedResults.Ok(response);
    }

    #endregion
}
