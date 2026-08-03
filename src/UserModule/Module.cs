using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using ModularMonolith.Template.SharedKernel.Compliance;
using ModularMonolith.Template.SharedKernel.Compliance.Redaction;
using ModularMonolith.Template.SharedKernel.Configuration;
using ModularMonolith.Template.SharedKernel.Modular;
using UserModule.Configuration;
using UserModule.Contracts.Requests;
using UserModule.Contracts.Responses;
using UserModule.Models;
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
        serviceCollection.AddScoped<IValidator<CreateUserRequest>, Validation.CreateUserRequestValidator>();
        serviceCollection.AddScoped<IValidator<UpdateUserRequest>, Validation.UpdateUserRequestValidator>();

        serviceCollection.AddCompliance(assemblies: typeof(Module).Assembly);
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints.MapGroup("/users");

        group.MapGet("/", Ok<IEnumerable<UserResponse>> (UserStore store, IModelRedactor<User> redactor, IRedactorProvider redactorProvider) =>
        {
            IEnumerable<UserResponse> users = store.GetAll()
                .Select(user => redactor.Redact(user, redactorProvider).To<UserResponse>());
            return TypedResults.Ok(users);
        });

        group.MapGet("/{id:int}", Results<Ok<UserResponse>, NotFound> (int id, UserStore store, IModelRedactor<User> redactor, IRedactorProvider redactorProvider) =>
        {
            User? user = store.Get(id);
            return user is null
                ? TypedResults.NotFound()
                : TypedResults.Ok(redactor.Redact(user, redactorProvider).To<UserResponse>());
        });

        group.MapPost("/", async Task<Results<Created<UserResponse>, ValidationProblem>> (CreateUserRequest request, UserStore store, IValidator<CreateUserRequest> validator) =>
        {
            ValidationResult validation = await validator.ValidateAsync(request);
            if (!validation.IsValid)
            {
                return TypedResults.ValidationProblem(validation.ToDictionary());
            }

            User created = store.Add(new User
            {
                Id = 0,
                FullName = request.FullName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
            });

            return TypedResults.Created($"/users/{created.Id}", created.To<UserResponse>());
        });

        group.MapPut("/{id:int}", async Task<Results<Ok<UserResponse>, NotFound, ValidationProblem>> (int id, UpdateUserRequest request, UserStore store, IValidator<UpdateUserRequest> validator) =>
        {
            ValidationResult validation = await validator.ValidateAsync(request);
            if (!validation.IsValid)
            {
                return TypedResults.ValidationProblem(validation.ToDictionary());
            }

            User? updated = store.Update(id, new User
            {
                Id = id,
                FullName = request.FullName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
            });

            return updated is null ? TypedResults.NotFound() : TypedResults.Ok(updated.To<UserResponse>());
        });

        group.MapDelete("/{id:int}", Results<NoContent, NotFound> (int id, UserStore store) =>
            store.Remove(id) ? TypedResults.NoContent() : TypedResults.NotFound());
    }

    #endregion
}