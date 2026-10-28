using ModularMonolith.Template.SharedKernel.Messaging;

namespace UserModule.Contracts.Events;

public sealed record UserCreatedIntegrationEvent : Message
{
    #region Property Declarations

    public required int Id { get; init; }

    public required string FullName { get; init; }

    public required string Email { get; init; }

    #endregion
}
