using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Template.SharedKernel.Outbox;
using ModularMonolith.Template.SharedKernel.Storage;

namespace UserModule.Infrastructure.Storage;

internal sealed class UserModuleOutboxStore([FromKeyedServices(Module.ServiceKey)] IDbConnectionFactory connectionFactory, IOutboxInterceptor outboxInterceptor) : SqlServerOutboxStore(connectionFactory, outboxInterceptor);
