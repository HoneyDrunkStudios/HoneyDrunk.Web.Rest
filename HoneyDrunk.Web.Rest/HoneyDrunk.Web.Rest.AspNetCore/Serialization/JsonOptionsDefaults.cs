using System.Text.Json;
using System.Text.Json.Serialization;

namespace HoneyDrunk.Web.Rest.AspNetCore.Serialization;

/// <summary>
/// Provides default JSON serializer options consistent with HoneyDrunk REST contracts.
/// </summary>
public static class JsonOptionsDefaults
{
    /// <summary>
    /// Gets the default <see cref="JsonSerializerOptions"/> for HoneyDrunk REST APIs.
    /// </summary>
    public static JsonSerializerOptions SerializerOptions { get; } = CreateOptions();

    /// <summary>
    /// Configures the provided <see cref="JsonSerializerOptions"/> with HoneyDrunk defaults.
    /// </summary>
    /// <param name="options">The options to configure.</param>
    public static void Configure(JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.PropertyNameCaseInsensitive = true;
        options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.WriteIndented = false;
        options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
    }

    private static JsonSerializerOptions CreateOptions()
    {
        JsonSerializerOptions options = new();
        Configure(options);
        return options;
    }
}
