// Copyright (c) Microsoft. All rights reserved.
// MCP Tools for Configuration Get/Update operations

// ============================================================================
// EXERCISE 3: Define MCP Tools
// ============================================================================
// In this exercise, you will create MCP tools that can be called by AI agents.
// Tools are methods decorated with [McpServerTool] attribute.
//
// TODO: Uncomment the code below step by step as instructed in EXERCISES.md
// ============================================================================

using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;

namespace McpLocal.Tools;

/// <summary>
/// MCP Tools for demonstrating Get and Update operations.
/// These tools are automatically discovered and exposed by the MCP server.
/// </summary>
// ============================================================================
// STEP 3.1: Mark this class as containing MCP tools
// Uncomment the attribute below
// ============================================================================
// [McpServerToolType]
public sealed class ConfigurationTools
{
    private readonly ConfigurationStore _store;

    public ConfigurationTools(ConfigurationStore store)
    {
        _store = store;
    }

    // ============================================================================
    // STEP 3.2: Create the GetConfig tool
    // This tool retrieves configuration values. Uncomment the method below.
    // ============================================================================
    // [McpServerTool]
    // [Description("Gets a configuration value by key. If no key is provided, returns all configurations. Example keys: app.name, app.version, feature.darkMode, user.theme")]
    // public string GetConfig(
    //     [Description("The configuration key to retrieve. Leave empty to get all configurations.")] 
    //     string? key = null)
    // {
    //     Console.Error.WriteLine($"GetConfig called with key: '{key ?? "(all)"}'");
    // 
    //     if (string.IsNullOrWhiteSpace(key))
    //     {
    //         var allResult = _store.GetAllConfigurations();
    //         Console.Error.WriteLine($"Returning {allResult.Count} configurations");
    //         return JsonSerializer.Serialize(allResult, new JsonSerializerOptions { WriteIndented = true });
    //     }
    // 
    //     var result = _store.GetConfiguration(key);
    //     Console.Error.WriteLine($"GetConfig result: {result.Message}");
    //     return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
    // }

    // ============================================================================
    // STEP 3.3: Create the UpdateConfig tool
    // This tool updates configuration values. Uncomment the method below.
    // ============================================================================
    // [McpServerTool]
    // [Description("Updates a configuration value. Creates the key if it doesn't exist. Example: key='feature.darkMode', value='true'")]
    // public string UpdateConfig(
    //     [Description("The configuration key to update (required).")] 
    //     string key,
    //     [Description("The new value for the configuration (required).")] 
    //     string value)
    // {
    //     Console.Error.WriteLine($"UpdateConfig called: key='{key}', value='{value}'");
    // 
    //     if (string.IsNullOrWhiteSpace(key))
    //     {
    //         var errorResult = new ConfigResult
    //         {
    //             Success = false,
    //             Key = key ?? "",
    //             Value = null,
    //             Message = "Key cannot be empty"
    //         };
    //         return JsonSerializer.Serialize(errorResult, new JsonSerializerOptions { WriteIndented = true });
    //     }
    // 
    //     var result = _store.UpdateConfiguration(key, value);
    //     Console.Error.WriteLine($"UpdateConfig result: {result.Message}");
    //     return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
    // }
}
