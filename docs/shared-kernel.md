# Shared Kernel

[← Docs index](README.md)

`ModularMonolith.Template.SharedKernel` holds

## Areas

- **Results** (`Results/`) — `Result` / `Result<T>` and `Error` for explicit success/failure handling instead of exceptions.
- **Validation** (`Validation/`) — FluentValidation integration and a `ValidationFilter` for endpoints.
- **Modular** (`Modular/`) — the [`IModule`](architecture.md#the-module-contract) abstraction and module-registration helpers.
- **Messaging** (`Messaging/`) — `IMessageDispatcher`, the in-process dispatcher and handler registration extensions.
- **Outbox** (`Outbox/`) — transactional outbox processor and abstractions.
- **Inbox** (`Inbox/`) — inbox store to deduplicate inbound messages.
- **Idempotency** (`Idempotency/`) — idempotency filter/keys to make request and message processing safe to retry.
- **Persistence / Storage** (`Persistence/`, `Storage/`) — connection factories, database initialisers and SQL Server store implementations.
- **Paging** (`Paging/`) — paging service and paged-result models.
- **Auditing** (`Auditing/`) — audit-log filter capturing sensitive operations.
- **Compliance** (`Compliance/`) — attributes such as `[PersonalData]` to mark PII.
- **Security** (`Security/`) — shared security abstractions and helpers.
- **Http** (`Http/`) — HTTP helpers and constants.
- **Serialisation** (`Serialisation/`) — shared serialization configuration.
- **Configuration** (`Configuration/`) — validated options helpers (`AddValidatedOptions`).
- **Features** (`Features/`) — feature-flag abstractions.
- **Azure** (`Azure/`) — integrations such as Key Vault configuration.

## Example

`Result`/`Result<T>` and `Error` model success and failure explicitly. Implicit conversions keep call sites clean:

```csharp
public async Task<Result<UserResponse>> UpdateAsync(int id, UpdateUserRequest request, CancellationToken cancellationToken = default)
{
    User? existing = await store.FindAsync(id, cancellationToken);
    if (existing is null)
    {
        return Error.NotFound("user.not-found", $"User {id} was not found.");
    }

    User updated = existing with { FullName = request.FullName, Email = request.Email };
    await store.UpdateAsync(updated, cancellationToken);

    return updated.ToResponse(); // implicit conversion from UserResponse to Result<UserResponse>
}
```

A request validator (used by `ValidationFilter<T>` on endpoints):

```csharp
internal sealed class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        RuleFor(request => request.FullName).NotEmpty().MaximumLength(200);
        RuleFor(request => request.Email).NotEmpty().EmailAddress().MaximumLength(320);
        RuleFor(request => request.PhoneNumber).MaximumLength(30).When(request => request.PhoneNumber is not null);
    }
}
```

## Error Catalogue

`Error` is created through factory methods, each carrying an `ErrorType`. At the endpoint boundary, `error.ToProblem()` maps the type to an HTTP status and returns a `ProblemHttpResult`:

| Factory | `ErrorType` | HTTP status |
| --- | --- | --- |
| `Error.Validation(code, message)` | `Validation` | 400 Bad Request |
| `Error.Unauthorised(code, message)` | `Unauthorised` | 401 Unauthorized |
| `Error.Forbidden(code, message)` | `Forbidden` | 403 Forbidden |
| `Error.NotFound(code, message)` | `NotFound` | 404 Not Found |
| `Error.Conflict(code, message)` | `Conflict` | 409 Conflict |
| `Error.Failure(code, message)` | `Failure` | 500 Internal Server Error |

```csharp
// In a service — return an Error (implicitly converted to Result<T>)
return Error.NotFound("user.not-found", $"User {id} was not found.");

// In an endpoint — translate a failed Result to an HTTP problem response
return result.IsSuccess ? TypedResults.Ok(result.Value) : result.Error.ToProblem();
```

## Related Docs

- [Architecture](architecture.md)
- [Messaging & Reliability](messaging.md)
- [API Host](api-host.md)
