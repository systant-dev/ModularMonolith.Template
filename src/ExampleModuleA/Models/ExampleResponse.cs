using System.Diagnostics.CodeAnalysis;

namespace ExampleModuleA.Models;

internal sealed record ExampleResponse
{
    #region Constructor Declarations

    [SetsRequiredMembers]
    public ExampleResponse()
    {
    }

    #endregion

    #region Property Declarations

    public required int Id { get; init; }
    public string? Name { get; init; }

    #endregion
}