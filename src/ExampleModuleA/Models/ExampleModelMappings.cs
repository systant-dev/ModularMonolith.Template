using ModularMonolith.Template.MappingGen;

namespace ExampleModuleA.Models;

internal static partial class ExampleModelMappings
{
    #region Public Method Declarations

    [Map]
    public static partial TTarget To<TTarget>(this ExampleModel source);

    #endregion
}