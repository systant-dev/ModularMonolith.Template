using System.Text.Json;
using ModularMonolith.Template.SharedKernel.Messaging;
using ModularMonolith.Template.SharedKernel.Serialisation;

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

    #endregion

    #region Public Method Declarations

    public static OutboxMessage Create(IMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);

        Type messageType = message.GetType();

        return new OutboxMessage
        {
            Id = Guid.NewGuid(),
            OccurredOnUtc = DateTimeOffset.UtcNow,
            Type = messageType.AssemblyQualifiedName ?? messageType.FullName ?? messageType.Name,
            Payload = JsonSerializer.Serialize(message, messageType, JsonDefaults.Web)
        };
    }

    public IMessage? Deserialise()
    {
        Type? messageType = System.Type.GetType(Type);
        if (messageType is null)
        {
            return null;
        }

        return JsonSerializer.Deserialize(Payload, messageType, JsonDefaults.Web) as IMessage;
    }

    #endregion
}
