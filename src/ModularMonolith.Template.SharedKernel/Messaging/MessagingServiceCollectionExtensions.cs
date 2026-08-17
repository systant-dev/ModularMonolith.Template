using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace ModularMonolith.Template.SharedKernel.Messaging;

public static class MessagingServiceCollectionExtensions
{
    #region Field Declarations

    private static readonly ConditionalWeakTable<IServiceCollection, MessageContractRegistry> Registries = [];

    #endregion

    extension(IServiceCollection serviceCollection)
    {
        public IServiceCollection AddInProcessMessaging()
        {
            serviceCollection.AddSingleton<IMessageDispatcher, InProcessMessageDispatcher>();
            serviceCollection.GetOrAddMessageContractRegistry();
            return serviceCollection;
        }

        public IServiceCollection AddMessageHandler<TMessage, THandler>() where TMessage : IMessage where THandler : class, IMessageHandler<TMessage>
        {
            // Register the concrete handler, then expose IMessageHandler<TMessage> as the logging
            // decorator wrapping it so every handler gets consistent start/finish/error logging
            // without changing the handler implementations themselves.
            serviceCollection.AddScoped<THandler>();
            serviceCollection.AddScoped<IMessageHandler<TMessage>>(provider =>
                new LoggingMessageHandlerDecorator<TMessage>(
                    provider.GetRequiredService<THandler>(),
                    provider.GetRequiredService<ILogger<LoggingMessageHandlerDecorator<TMessage>>>()));
            serviceCollection.GetOrAddMessageContractRegistry().Register<TMessage>();
            return serviceCollection;
        }

        public IServiceCollection AddIdempotentMessageHandler<TMessage, THandler>()
            where TMessage : IMessage where THandler : class, IMessageHandler<TMessage>
        {
            // Same as AddMessageHandler but additionally wraps the handler in the inbox idempotency
            // decorator, so at-least-once delivery is de-duplicated centrally on the message's own
            // MessageId instead of inside the handler. The consumer name is the handler type name.
            serviceCollection.AddScoped<THandler>();
            serviceCollection.AddScoped<IMessageHandler<TMessage>>(provider =>
            {
                LoggingMessageHandlerDecorator<TMessage> logging = new(
                    provider.GetRequiredService<THandler>(),
                    provider.GetRequiredService<ILogger<LoggingMessageHandlerDecorator<TMessage>>>());

                return new InboxIdempotencyMessageHandlerDecorator<TMessage>(
                    logging,
                    provider.GetRequiredService<Inbox.IInboxStore>(),
                    typeof(THandler).Name,
                    provider.GetRequiredService<ILogger<InboxIdempotencyMessageHandlerDecorator<TMessage>>>());
            });
            serviceCollection.GetOrAddMessageContractRegistry().Register<TMessage>();
            return serviceCollection;
        }

        public IServiceCollection AddMessageContract<TMessage>() where TMessage : IMessage
        {
            serviceCollection.GetOrAddMessageContractRegistry().Register<TMessage>();
            return serviceCollection;
        }
    }

    #region Internal Method Declarations

    internal static MessageContractRegistry GetOrAddMessageContractRegistry(this IServiceCollection serviceCollection)
    {
        // The registry is cached per service collection so repeated handler/contract registrations
        // are O(1) instead of scanning every descriptor each time (avoids O(n^2) startup wiring).
        return Registries.GetValue(serviceCollection, static collection =>
        {
            ServiceDescriptor? descriptor = collection.FirstOrDefault(descriptor => descriptor.ServiceType == typeof(IMessageContractRegistry));

            if (descriptor is not null)
            {
                return descriptor.ImplementationInstance as MessageContractRegistry
                    ?? throw new InvalidOperationException(
                        $"{nameof(IMessageContractRegistry)} is already registered with an incompatible implementation. " +
                        "Register message contracts through AddInProcessMessaging/AddMessageHandler/AddMessageContract instead of registering the registry manually.");
            }

            MessageContractRegistry registry = new();
            collection.TryAddSingleton<IMessageContractRegistry>(registry);
            return registry;
        });
    }

    #endregion
}
