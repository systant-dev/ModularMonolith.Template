using ModularMonolith.Template.SharedKernel.Messaging;

namespace UserModule.Contracts.Events;

public sealed record UserDeletedIntegrationEvent : Message
{
    #region Property Declarations

    public required int Id { get; init; }

    #endregion
}
