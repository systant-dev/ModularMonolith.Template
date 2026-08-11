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
using ModularMonolith.Template.SharedKernel.Messaging;
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

        // This template uses Dapper over SQL Server (LocalDB in the example) so the persisted SQL
        // matches the production dialect. Automated tests substitute an in-memory IAccountStore, so no
        // database server is required to run them.
        serviceCollection.AddSingleton<IDbConnectionFactory>(provider =>
        {
            AccountsModuleOptions options = provider.GetRequiredService<IOptions<AccountsModuleOptions>>().Value;
            return new SqlServerConnectionFactory(options.ConnectionString);
        });

        serviceCollection.AddScoped<IModuleDatabaseInitialiser, AccountsModuleDatabaseInitialiser>();
        serviceCollection.AddOutboxStore<SqlServerAccountStore>();
        serviceCollection.AddScoped<IAccountStore>(provider => provider.GetRequiredService<SqlServerAccountStore>());

        serviceCollection.AddScoped<IAccountService, AccountService>();
        serviceCollection.AddScoped<IValidator<CreateAccountRequest>, Validation.CreateAccountRequestValidator>();
        serviceCollection.AddScoped<IValidator<UpdateAccountRequest>, Validation.UpdateAccountRequestValidator>();

        // React to users being created in the User module by opening a savings account for them.
        serviceCollection.AddMessageHandler<UserCreatedIntegrationEvent, UserCreatedHandler>();
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
        group.MapPost("/", AccountEndpoints.Create).AddEndpointFilter<ValidationFilter<CreateAccountRequest>>().RequireAuthorization(AuthorisationPolicies.WriteAccess);
        group.MapPut("/{id:int}", AccountEndpoints.Update).AddEndpointFilter<ValidationFilter<UpdateAccountRequest>>().RequireAuthorization(AuthorisationPolicies.WriteAccess);
        group.MapDelete("/{id:int}", AccountEndpoints.Delete).RequireAuthorization(AuthorisationPolicies.WriteAccess);
    }

    #endregion
}