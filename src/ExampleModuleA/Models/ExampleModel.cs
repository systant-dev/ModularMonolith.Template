using ModularMonolith.Template.MappingGen;

namespace ExampleModuleA.Models;

internal sealed partial record ExampleModel
{
    #region Property Declarations

    public required int Id { get; init; }
    public string? Name { get; init; }

    #endregion

    #region Public Method Declarations

    public void Map()
    {
        _ = this.To<ExampleResponse>();
    }

    #endregion
}

internal static partial class ExampleModelMappings
{
    #region Public Method Declarations

    [Map]
    public static partial TTarget To<TTarget>(this ExampleModel source);

    #endregion
}