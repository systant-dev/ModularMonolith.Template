using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Asp.Versioning;
using Asp.Versioning.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Template.SharedKernel.Configuration;
using ModularMonolith.Template.SharedKernel.Modular;
using ModularMonolith.Template.SharedKernel.Outbox;
using ModularMonolith.Template.SharedKernel.Security;
using ModularMonolith.Template.SharedKernel.Validation;
using UserModule.Configuration;
using UserModule.Contracts.Requests;
using UserModule.Endpoints;
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

        serviceCollection.AddModuleDbContext<UserDbContext>((_, options) =>
        {
            // This template uses the EF Core in-memory provider so the example runs without
            // requiring a real database. Swap this for options.UseSqlServer(...) (or another
            // relational provider) for production scenarios.
            options.UseInMemoryDatabase(nameof(UserModule));
        });

        serviceCollection.AddScoped<IUserService, UserService>();
        serviceCollection.AddScoped<IValidator<CreateUserRequest>, Validation.CreateUserRequestValidator>();
        serviceCollection.AddScoped<IValidator<UpdateUserRequest>, Validation.UpdateUserRequestValidator>();
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        ApiVersionSet apiVersionSet = endpoints.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1, 0))
            .ReportApiVersions()
            .Build();

        RouteGroupBuilder group = endpoints.MapGroup("/v{version:apiVersion}/users")
            .WithApiVersionSet(apiVersionSet)
            .HasApiVersion(new ApiVersion(1, 0));

        group.RequireRateLimiting("default");
        group.WithRequestTimeout("default");

        group.MapGet("/", UserEndpoints.GetAll).RequireAuthorization(AuthorizationPolicies.ReadAccess);
        group.MapGet("/{id:int}", UserEndpoints.Get).RequireAuthorization(AuthorizationPolicies.ReadAccess);
        group.MapPost("/", UserEndpoints.Create).AddEndpointFilter<ValidationFilter<CreateUserRequest>>().RequireAuthorization(AuthorizationPolicies.WriteAccess);
        group.MapPut("/{id:int}", UserEndpoints.Update).AddEndpointFilter<ValidationFilter<UpdateUserRequest>>().RequireAuthorization(AuthorizationPolicies.WriteAccess);
        group.MapDelete("/{id:int}", UserEndpoints.Delete).RequireAuthorization(AuthorizationPolicies.WriteAccess);
    }

    #endregion
}