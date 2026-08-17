namespace ModularMonolith.Template.SharedKernel.Messaging;

public abstract record Message : IMessage
{
    #region Property Declarations

    public Guid MessageId { get; init; } = Guid.CreateVersion7();

    #endregion
}
