namespace ModularMonolith.Template.SharedKernel.Messaging;

public interface IMessage
{
    #region Property Declarations

    Guid MessageId { get; }

    #endregion
}
