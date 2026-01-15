// Copyright (c) Microsoft. All rights reserved.
// MCP Tools for Configuration Get/Update operations

using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;

namespace McpLocal.Tools;

/// <summary>
/// MCP Tools for demonstrating Get and Update operations.
/// These tools are automatically discovered and exposed by the MCP server.
/// </summary>
[McpServerToolType]
public sealed class ConfigurationTools
{
    private readonly ConfigurationStore _store;

    public ConfigurationTools(ConfigurationStore store)
    {
        _store = store;
    }

    /// <summary>
    /// Gets a configuration value by key, or all configurations if no key is provided.
    /// </summary>
    /// <param name="key">The configuration key to retrieve. If empty or null, returns all configurations.</param>
    /// <returns>JSON string with the configuration result.</returns>
    [McpServerTool]
    [Description("Gets a configuration value by key. If no key is provided, returns all configurations. Example keys: app.name, app.version, feature.darkMode, user.theme")]
    public string GetConfig(
        [Description("The configuration key to retrieve. Leave empty to get all configurations.")] 
        string? key = null)
    {
        Console.Error.WriteLine($"GetConfig called with key: '{key ?? "(all)"}'");

        if (string.IsNullOrWhiteSpace(key))
        {
            var allResult = _store.GetAllConfigurations();
            Console.Error.WriteLine($"Returning {allResult.Count} configurations");
            return JsonSerializer.Serialize(allResult, new JsonSerializerOptions { WriteIndented = true });
        }

        var result = _store.GetConfiguration(key);
        Console.Error.WriteLine($"GetConfig result: {result.Message}");
        return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
    }

    /// <summary>
    /// Updates a configuration value.
    /// </summary>
    /// <param name="key">The configuration key to update.</param>
    /// <param name="value">The new value for the configuration.</param>
    /// <returns>JSON string with the update result.</returns>
    [McpServerTool]
    [Description("Updates a configuration value. Creates the key if it doesn't exist. Example: key='feature.darkMode', value='true'")]
    public string UpdateConfig(
        [Description("The configuration key to update (required).")] 
        string key,
        [Description("The new value for the configuration (required).")] 
        string value)
    {
        Console.Error.WriteLine($"UpdateConfig called: key='{key}', value='{value}'");

        if (string.IsNullOrWhiteSpace(key))
        {
            var errorResult = new ConfigResult
            {
                Success = false,
                Key = key ?? "",
                Message = "Configuration key is required"
            };
            return JsonSerializer.Serialize(errorResult, new JsonSerializerOptions { WriteIndented = true });
        }

        if (string.IsNullOrWhiteSpace(value))
        {
            var errorResult = new ConfigResult
            {
                Success = false,
                Key = key,
                Message = "Configuration value is required"
            };
            return JsonSerializer.Serialize(errorResult, new JsonSerializerOptions { WriteIndented = true });
        }

        var result = _store.UpdateConfiguration(key, value);
        Console.Error.WriteLine($"UpdateConfig result: {result.Message}");
        return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
    }
}
