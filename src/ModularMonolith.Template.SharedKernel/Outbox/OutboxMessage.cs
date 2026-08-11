using ModularMonolith.Template.SharedKernel.Messaging;
using ModularMonolith.Template.SharedKernel.Serialisation;
using System.Reflection;
using System.Text.Json;

namespace ModularMonolith.Template.SharedKernel.Outbox;

public sealed class OutboxMessage
{
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

        return new OutboxMessage
        {
            Id = Guid.CreateVersion7(),
            OccurredOnUtc = DateTimeOffset.UtcNow,
            Type = ResolveContractName(messageType),
            Payload = JsonSerializer.Serialize(message, messageType, JsonDefaults.Web)
        };
    }

    public IMessage? Deserialise(IMessageContractRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);

        if (!registry.TryResolve(Type, out Type? messageType))
        {
            return null;
        }

        return JsonSerializer.Deserialize(Payload, messageType, JsonDefaults.Web) as IMessage;
    }

    #endregion

    #region Private Method Declarations

    private static string ResolveContractName(Type messageType)
    {
        MessageContractAttribute? attribute = messageType.GetCustomAttribute<MessageContractAttribute>(inherit: false);
        return attribute?.Name ?? messageType.FullName ?? messageType.Name;
    }

    #endregion
}
