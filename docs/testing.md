# Testing

[← Docs index](README.md)

The `tests/` folder mirrors

## Test Projects

- **`ModularMonolith.Template.Api.Tests`** — host / integration tests for the API pipeline and end-to-end module behaviour.
- **`ModularMonolith.Template.UserModule.Tests`** / **`ModularMonolith.Template.AccountsModule.Tests`** — per-module unit tests for services, validation and handlers.
- **`ModularMonolith.Template.SharedKernel.Tests`** — tests for cross-cutting primitives (results, paging, messaging, etc.).
- **`ModularMonolith.Template.ModularGen.Tests`** / **`ModularMonolith.Template.MappingGen.Tests`** — snapshot/behaviour tests for the source generators.
- **`ModularMonolith.Template.SourceGenerators.Shared.Tests`** — tests for the shared generator infrastructure.
- **`ModularMonolith.Template.Analysers.Tests`** — verify analyser diagnostics fire (and don't fire) correctly.
- **`ModularMonolith.Template.SourceGenerators.TestKit`** — shared helpers/harness for testing generators and analysers.

## Running

```powershell
dotnet test ModularMonolith.Template.slnx
```

Run a single suite:

```powershell
dotnet test tests/ModularMonolith.Template.UserModule.Tests
```

## Example

Module tests use **xUnit** with **FluentAssertions** and an in-memory store, so services can be tested without a database:

```csharp
[Component(TestComponent.UserModule)]
[Category(TestCategory.Modular)]
public sealed class UserServiceTests
{
    [Fact]
    public async Task Create_WhenEmailAlreadyExists_ReturnsConflict()
    {
        UserService service = await CreateService();
        await service.CreateAsync(NewRequest("First User", "dupe@example.com"), TestContext.Current.CancellationToken);

        Result<UserResponse> result = await service.CreateAsync(
            NewRequest("Second User", "DUPE@example.com"),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("user.email-conflict");
    }
}
```

## Related Docs

- [Architecture](architecture.md)
- [Source Generators](source-generators.md)
- [Analysers](analysers.md)
