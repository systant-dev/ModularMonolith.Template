using ModularMonolith.Template.SharedKernel.Messaging;

namespace ModularMonolith.Template.Api.Messaging;

public static partial class MessageDispatchServiceCollectionExtensions
{
    extension(IServiceCollection serviceCollection)
    {
        public IServiceCollection AddMessaging()
        {
            serviceCollection.AddInProcessMessaging();
            RegisterGeneratedMessageDispatch(serviceCollection);
            return serviceCollection;
        }
    }

    #region Private Method Declarations

    [MessageDispatchMapTarget]
    private static partial void RegisterGeneratedMessageDispatch(IServiceCollection serviceCollection);

    #endregion
}