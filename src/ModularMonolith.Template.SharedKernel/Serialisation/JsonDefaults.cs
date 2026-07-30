using System.Text.Json;

namespace ModularMonolith.Template.SharedKernel.Serialisation;

public static class JsonDefaults
{
    #region Property Declarations

    public static JsonSerializerOptions Web { get; } = new(JsonSerializerDefaults.Web);

    #endregion
}