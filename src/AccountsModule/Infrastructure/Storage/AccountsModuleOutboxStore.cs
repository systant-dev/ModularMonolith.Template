using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Template.SharedKernel.Outbox;
using ModularMonolith.Template.SharedKernel.Storage;

namespace AccountsModule.Infrastructure.Storage;

internal sealed class AccountsModuleOutboxStore([FromKeyedServices(Module.ServiceKey)] IDbConnectionFactory connectionFactory, IOutboxInterceptor outboxInterceptor) : SqlServerOutboxStore(connectionFactory, outboxInterceptor);
