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

        IEnumerable<Task<IReadOnlyList<OutboxDeadLetterResponse>>> fetchTasks = registry.StoreTypes
            .Select(storeType => GetStoreDeadLettersAsync(scopeFactory, storeType, outboxOptions, limit, cancellationToken));

        IReadOnlyList<OutboxDeadLetterResponse>[] results = await Task.WhenAll(fetchTasks);

        IReadOnlyList<OutboxDeadLetterResponse> response =
        [
            .. results.SelectMany(storeDeadLetters => storeDeadLetters)
                      .OrderBy(deadLetter => deadLetter.OccurredOnUtc)
        ];

        return TypedResults.Ok(response);
    }

    private static async Task<IReadOnlyList<OutboxDeadLetterResponse>> GetStoreDeadLettersAsync(IServiceScopeFactory scopeFactory,
                                                                                                Type storeType,
                                                                                                OutboxOptions outboxOptions,
                                                                                                int limit,
                                                                                                CancellationToken cancellationToken)
    {
        await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();

        if (scope.ServiceProvider.GetRequiredService(storeType) is not IOutboxStore store)
        {
            return [];
        }

        IReadOnlyList<OutboxMessage> messages = await store.GetDeadLetteredAsync(outboxOptions.MaxAttempts, limit, cancellationToken);

        return
        [
            .. messages.Select(message => new OutboxDeadLetterResponse(
                message.Id,
                message.Type,
                message.OccurredOnUtc,
                message.Attempts,
                message.Error))
        ];
    }

    #endregion
}
