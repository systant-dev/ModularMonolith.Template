using System.Text.Json;

namespace ModularMonolith.Template.SharedKernel.Serialisation;

public static class JsonDefaults
{
    #region Property Declarations

    /// <summary>
    /// Shared, read-only <see cref="JsonSerializerOptions"/> configured with web defaults.
    /// The instance is frozen via <see cref="JsonSerializerOptions.MakeReadOnly()"/> so it can be
    /// safely reused across the application without risk of concurrent mutation.
    /// </summary>
    public static JsonSerializerOptions Web { get; } = CreateWeb();

    #endregion

    #region Private Method Declarations

    private static JsonSerializerOptions CreateWeb()
    {
        JsonSerializerOptions options = new(JsonSerializerDefaults.Web);
        options.MakeReadOnly();
        return options;
    }

    #endregion
}