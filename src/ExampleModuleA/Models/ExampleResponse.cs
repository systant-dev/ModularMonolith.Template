namespace ExampleModuleA.Models;

internal sealed record ExampleResponse
{
    #region Property Declarations

    public required int Id { get; init; }

    public string? Name { get; init; }

    #endregion
}