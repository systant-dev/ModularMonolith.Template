using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ModularMonolith.Template.SharedKernel.Messaging;

public static class MessagingServiceCollectionExtensions
{
    extension(IServiceCollection serviceCollection)
    {
        public IServiceCollection AddInProcessMessaging()
        {
            serviceCollection.AddSingleton<IMessageDispatcher, InProcessMessageDispatcher>();
            serviceCollection.GetOrAddMessageContractRegistry();
            return serviceCollection;
        }

        public IServiceCollection AddMessageHandler<TMessage, THandler>()
            where TMessage : IMessage
            where THandler : class, IMessageHandler<TMessage>
        {
            serviceCollection.AddScoped<IMessageHandler<TMessage>, THandler>();
            serviceCollection.GetOrAddMessageContractRegistry().Register(typeof(TMessage));
            return serviceCollection;
        }

        public IServiceCollection AddMessageContract<TMessage>()
            where TMessage : IMessage
        {
            serviceCollection.GetOrAddMessageContractRegistry().Register(typeof(TMessage));
            return serviceCollection;
        }
    }

    #region Internal Method Declarations

    internal static MessageContractRegistry GetOrAddMessageContractRegistry(this IServiceCollection serviceCollection)
    {
        MessageContractRegistry? existing = serviceCollection
            .FirstOrDefault(descriptor => descriptor.ServiceType == typeof(IMessageContractRegistry))?
            .ImplementationInstance as MessageContractRegistry;

        if (existing is not null)
        {
            return existing;
        }

        MessageContractRegistry registry = new();
        serviceCollection.TryAddSingleton<IMessageContractRegistry>(registry);
        return registry;
    }

    #endregion
}
