using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Template.SharedKernel.Configuration;
using ModularMonolith.Template.SharedKernel.Modular;
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

        serviceCollection.AddSingleton<UserStore>();
        serviceCollection.AddScoped<IUserService, UserService>();
        serviceCollection.AddScoped<IValidator<CreateUserRequest>, Validation.CreateUserRequestValidator>();
        serviceCollection.AddScoped<IValidator<UpdateUserRequest>, Validation.UpdateUserRequestValidator>();
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints.MapGroup("/v{version:apiVersion}/users");
        group.RequireRateLimiting("default");
        group.WithRequestTimeout("default");

        group.MapGet("/", UserEndpoints.GetAll);
        group.MapGet("/{id:int}", UserEndpoints.Get);
        group.MapPost("/", UserEndpoints.Create).AddEndpointFilter<ValidationFilter<CreateUserRequest>>();
        group.MapPut("/{id:int}", UserEndpoints.Update).AddEndpointFilter<ValidationFilter<UpdateUserRequest>>();
        group.MapDelete("/{id:int}", UserEndpoints.Delete);
    }

    #endregion
}