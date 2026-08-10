using ModularMonolith.Template.SharedKernel.Messaging;

namespace AccountsModule.Contracts.Events;

public sealed record AccountOpenedIntegrationEvent : IMessage
{
    #region Property Declarations

    public required int AccountId { get; init; }

    public required int UserId { get; init; }

    public required string AccountNumber { get; init; }

    #endregion
}
