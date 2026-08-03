using System.Text.Json;

namespace ModularMonolith.Template.SharedKernel.Serialisation;

public static class JsonDefaults
{
    #region Property Declarations

    public static JsonSerializerOptions Web { get; } = CreateWeb();

    #endregion

    #region Private Method Declarations

    private static JsonSerializerOptions CreateWeb()
    {
        JsonSerializerOptions options = new(JsonSerializerDefaults.Web);
        options.MakeReadOnly(populateMissingResolver: true);
        return options;
    }

    #endregion
}