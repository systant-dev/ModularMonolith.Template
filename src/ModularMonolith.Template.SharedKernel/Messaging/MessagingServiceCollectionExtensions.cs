using Microsoft.Extensions.DependencyInjection;

namespace ModularMonolith.Template.SharedKernel.Messaging;

public static class MessagingServiceCollectionExtensions
{
    extension(IServiceCollection serviceCollection)
    {
        public IServiceCollection AddInProcessMessaging()
        {
            serviceCollection.AddSingleton<IMessageDispatcher, InProcessMessageDispatcher>();
            return serviceCollection;
        }

        public IServiceCollection AddMessageHandler<TMessage, THandler>()
            where TMessage : IMessage
            where THandler : class, IMessageHandler<TMessage>
        {
            serviceCollection.AddScoped<IMessageHandler<TMessage>, THandler>();
            return serviceCollection;
        }
    }
}
