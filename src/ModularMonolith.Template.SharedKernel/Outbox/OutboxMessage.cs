using ModularMonolith.Template.SharedKernel.Messaging;
using ModularMonolith.Template.SharedKernel.Messaging.Contracts;
using ModularMonolith.Template.SharedKernel.Serialisation;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace ModularMonolith.Template.SharedKernel.Outbox;

public sealed class OutboxMessage
{
    #region Field Declarations

    private static readonly ConcurrentDictionary<Type, JsonTypeInfo> TypeInfoCache = new();

    #endregion

    #region Property Declarations

    public Guid Id { get; init; }

    public required string Type { get; init; }

    public required string Payload { get; init; }

    public DateTimeOffset OccurredOnUtc { get; init; }

    public DateTimeOffset? ProcessedOnUtc { get; set; }

    public int Attempts { get; set; }

    public string? Error { get; set; }

    public DateTimeOffset? NextAttemptOnUtc { get; set; }

    #endregion

    #region Public Method Declarations

    public static OutboxMessage Create(IMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);

        Type messageType = message.GetType();
        JsonTypeInfo jsonTypeInfo = GetTypeInfo(messageType);
        OutboxMessage outboxMessage = new()
        {
            Id = Guid.CreateVersion7(),
            OccurredOnUtc = DateTimeOffset.UtcNow,
            Type = MessageContractAttribute.ResolveContractName(messageType),
            Payload = JsonSerializer.Serialize(message, jsonTypeInfo)
        };

        return outboxMessage;
    }

    public IMessage? Deserialise(IMessageContractRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);

        if (!registry.TryResolve(Type, out Type? messageType))
        {
            return null;
        }

        JsonTypeInfo jsonTypeInfo = GetTypeInfo(messageType);
        return JsonSerializer.Deserialize(Payload, jsonTypeInfo) as IMessage;
    }

    #endregion

    #region Private Method Declarations

    private static JsonTypeInfo GetTypeInfo(Type messageType)
    {
        return TypeInfoCache.GetOrAdd(messageType, static type => JsonDefaults.Web.GetTypeInfo(type));
    }

    #endregion
}
