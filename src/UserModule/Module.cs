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
using ModularMonolith.Template.SharedKernel.Messaging;
using ModularMonolith.Template.SharedKernel.Modular;
using ModularMonolith.Template.SharedKernel.Outbox;
using ModularMonolith.Template.SharedKernel.Security;
using ModularMonolith.Template.SharedKernel.Storage;
using ModularMonolith.Template.SharedKernel.Validation;
using UserModule.Configuration;
using UserModule.Contracts.Requests;
using UserModule.Endpoints;
using UserModule.Messaging;
using UserModule.Services;
using UserModule.Storage;

namespace UserModule;

[Module]
public sealed class Module : IModule
{
    #region Public Method Declarations

    public void RegisterServices(IServiceCollection serviceCollection)
    {
        serviceCollection.AddValidatedOptions<UserModuleOptions, UserModuleOptionsValidator>(nameof(UserModule));

        // This template uses Dapper over SQL Server (LocalDB in the example) so the persisted SQL
        // matches the production dialect. Automated tests substitute an in-memory IUserStore, so no
        // database server is required to run them.
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
        serviceCollection.AddScoped<IValidator<CreateUserRequest>, Validation.CreateUserRequestValidator>();
        serviceCollection.AddScoped<IValidator<UpdateUserRequest>, Validation.UpdateUserRequestValidator>();

        // React to savings accounts being opened for our users by the Accounts module.
        serviceCollection.AddMessageHandler<AccountOpenedIntegrationEvent, AccountOpenedHandler>();
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
        group.MapPost("/", UserEndpoints.Create).AddEndpointFilter<ValidationFilter<CreateUserRequest>>().RequireAuthorization(AuthorisationPolicies.WriteAccess);
        group.MapPut("/{id:int}", UserEndpoints.Update).AddEndpointFilter<ValidationFilter<UpdateUserRequest>>().RequireAuthorization(AuthorisationPolicies.WriteAccess);
        group.MapDelete("/{id:int}", UserEndpoints.Delete).RequireAuthorization(AuthorisationPolicies.WriteAccess);
    }

    #endregion
}
