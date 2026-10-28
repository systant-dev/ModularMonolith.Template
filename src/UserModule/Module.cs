using AccountsModule.Contracts.Events;
using Asp.Versioning;
using Asp.Versioning.Builder;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ModularMonolith.Template.SharedKernel.Configuration;
using ModularMonolith.Template.SharedKernel.Auditing;
using ModularMonolith.Template.SharedKernel.Features;
using ModularMonolith.Template.SharedKernel.Idempotency;
using ModularMonolith.Template.SharedKernel.Messaging;
using ModularMonolith.Template.SharedKernel.Modular;
using ModularMonolith.Template.SharedKernel.Outbox;
using ModularMonolith.Template.SharedKernel.Security;
using ModularMonolith.Template.SharedKernel.Storage;
using ModularMonolith.Template.SharedKernel.Validation;
using UserModule.Infrastructure.Configuration;
using UserModule.Contracts.Events;
using UserModule.Contracts.Requests;
using UserModule.Features.AccountOpened;
using UserModule.Features.Users;
using UserModule.Features.Users.Validation;
using UserModule.Infrastructure.Storage;

namespace UserModule;

[Module]
public sealed class Module : IModule
{
    #region Public Method Declarations

    public void RegisterServices(IServiceCollection serviceCollection)
    {
        serviceCollection.AddValidatedOptions<UserModuleOptions, UserModuleOptionsValidator>(nameof(UserModule));

        serviceCollection.AddSingleton<IDbConnectionFactory>(provider =>
        {
            UserModuleOptions options = provider.GetRequiredService<IOptions<UserModuleOptions>>().Value;
            SqlServerConnectionFactory connectionFactory = new(options.ConnectionString);
            return connectionFactory;
        });

        serviceCollection.AddScoped<IModuleDatabaseInitialiser, UserModuleDatabaseInitialiser>();
        serviceCollection.AddOutboxStore<SqlServerUserStore>();
        serviceCollection.AddScoped<IUserStore>(provider => provider.GetRequiredService<SqlServerUserStore>());

        serviceCollection.AddScoped<IUserService, UserService>();
        serviceCollection.AddScoped<IValidator<CreateUserRequest>, CreateUserRequestValidator>();
        serviceCollection.AddScoped<IValidator<UpdateUserRequest>, UpdateUserRequestValidator>();

        serviceCollection.AddMessageHandler<AccountOpenedIntegrationEvent, AccountOpenedHandler>();

        serviceCollection.AddMessageContract<UserUpdatedIntegrationEvent>();
        serviceCollection.AddMessageContract<UserDeletedIntegrationEvent>();
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        ApiVersion apiVersion = new(1, 0);

        ApiVersionSet apiVersionSet = endpoints.NewApiVersionSet()
                                               .HasApiVersion(apiVersion)
                                               .ReportApiVersions()
                                               .Build();

        RouteGroupBuilder group = endpoints.MapGroup("/v{version:apiVersion}/users")
                                           .WithApiVersionSet(apiVersionSet)
                                           .HasApiVersion(apiVersion);

        group.RequireRateLimiting("default");
        group.WithRequestTimeout("default");

        group.MapGet("/", UserEndpoints.GetAll).RequireAuthorization(AuthorisationPolicies.ReadAccess);
        group.MapGet("/{id:int}", UserEndpoints.Get).RequireAuthorization(AuthorisationPolicies.ReadAccess);

        group.MapPost("/", UserEndpoints.Create).AddEndpointFilter<ValidationFilter<CreateUserRequest>>()
                                                .AddEndpointFilter<IdempotencyFilter>()
                                                .AddEndpointFilter<AuditLogFilter>()
                                                .RequireAuthorization(AuthorisationPolicies.WriteAccess);

        group.MapPut("/{id:int}", UserEndpoints.Update).AddEndpointFilter<ValidationFilter<UpdateUserRequest>>()
                                                       .AddEndpointFilter<AuditLogFilter>()
                                                       .RequireAuthorization(AuthorisationPolicies.WriteAccess);

        group.MapDelete("/{id:int}", UserEndpoints.Delete).AddEndpointFilter<AuditLogFilter>()
                                                          .RequireFeature("UserModule.UserDeletion")
                                                          .RequireAuthorization(AuthorisationPolicies.WriteAccess);
    }

    #endregion
}
