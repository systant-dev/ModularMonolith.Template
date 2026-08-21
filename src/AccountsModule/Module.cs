using AccountsModule.Configuration;
using AccountsModule.Contracts.Requests;
using AccountsModule.Endpoints;
using AccountsModule.Messaging;
using AccountsModule.Services;
using AccountsModule.Storage;
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
using ModularMonolith.Template.SharedKernel.Inbox;
using ModularMonolith.Template.SharedKernel.Modular;
using ModularMonolith.Template.SharedKernel.Outbox;
using ModularMonolith.Template.SharedKernel.Security;
using ModularMonolith.Template.SharedKernel.Storage;
using ModularMonolith.Template.SharedKernel.Validation;
using UserModule.Contracts.Events;

namespace AccountsModule;

[Module]
public sealed class Module : IModule
{
    #region Public Method Declarations

    public void RegisterServices(IServiceCollection serviceCollection)
    {
        serviceCollection.AddValidatedOptions<AccountsModuleOptions, AccountsModuleOptionsValidator>(nameof(AccountsModule));

        serviceCollection.AddSingleton<IDbConnectionFactory>(provider =>
        {
            AccountsModuleOptions options = provider.GetRequiredService<IOptions<AccountsModuleOptions>>().Value;
            SqlServerConnectionFactory connectionFactory = new(options.ConnectionString);
            return connectionFactory;
        });

        serviceCollection.AddScoped<IModuleDatabaseInitialiser, AccountsModuleDatabaseInitialiser>();
        serviceCollection.AddOutboxStore<SqlServerAccountStore>();
        serviceCollection.AddScoped<IAccountStore>(provider => provider.GetRequiredService<SqlServerAccountStore>());
        serviceCollection.AddScoped<IInboxStore>(provider => provider.GetRequiredService<SqlServerAccountStore>());

        serviceCollection.AddScoped<IAccountService, AccountService>();
        serviceCollection.AddScoped<IValidator<CreateAccountRequest>, Validation.CreateAccountRequestValidator>();
        serviceCollection.AddScoped<IValidator<UpdateAccountRequest>, Validation.UpdateAccountRequestValidator>();

        serviceCollection.AddIdempotentMessageHandler<UserCreatedIntegrationEvent, UserCreatedHandler>();
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        ApiVersion apiVersion = new(1, 0);

        ApiVersionSet apiVersionSet = endpoints.NewApiVersionSet()
                                               .HasApiVersion(apiVersion)
                                               .ReportApiVersions()
                                               .Build();

        RouteGroupBuilder group = endpoints.MapGroup("/v{version:apiVersion}/accounts")
                                           .WithApiVersionSet(apiVersionSet)
                                           .HasApiVersion(apiVersion);

        group.RequireRateLimiting("default");
        group.WithRequestTimeout("default");

        group.MapGet("/", AccountEndpoints.GetAll).RequireAuthorization(AuthorisationPolicies.ReadAccess);
        group.MapGet("/{id:int}", AccountEndpoints.Get).RequireAuthorization(AuthorisationPolicies.ReadAccess);

        group.MapPost("/", AccountEndpoints.Create).AddEndpointFilter<ValidationFilter<CreateAccountRequest>>()
                                                   .AddEndpointFilter<IdempotencyFilter>()
                                                   .AddEndpointFilter<AuditLogFilter>()
                                                   .RequireAuthorization(AuthorisationPolicies.WriteAccess);

        group.MapPut("/{id:int}", AccountEndpoints.Update).AddEndpointFilter<ValidationFilter<UpdateAccountRequest>>()
                                                          .AddEndpointFilter<AuditLogFilter>()
                                                          .RequireAuthorization(AuthorisationPolicies.WriteAccess);

        group.MapDelete("/{id:int}", AccountEndpoints.Delete).AddEndpointFilter<AuditLogFilter>()
                                                             .RequireFeature("AccountsDeletion")
                                                             .RequireAuthorization(AuthorisationPolicies.WriteAccess);
    }

    #endregion
}
