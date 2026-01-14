// Copyright (c) Microsoft. All rights reserved.
// In-memory configuration store for MCP demo

namespace McpLocal.Tools;

/// <summary>
/// Simple in-memory configuration store for demonstrating MCP Get/Update operations.
/// </summary>
public class ConfigurationStore
{
    private readonly Dictionary<string, string> _configurations = new()
    {
        ["app.name"] = "MCP Workshop Demo",
        ["app.version"] = "1.0.0",
        ["app.environment"] = "development",
        ["feature.darkMode"] = "false",
        ["feature.notifications"] = "true",
        ["user.theme"] = "light",
        ["user.language"] = "en-US"
    };

    private readonly object _lock = new();

    /// <summary>
    /// Gets a configuration value by key.
    /// </summary>
    public ConfigResult GetConfiguration(string key)
    {
        lock (_lock)
        {
            if (_configurations.TryGetValue(key, out var value))
            {
                return new ConfigResult
                {
                    Success = true,
                    Key = key,
                    Value = value,
                    Message = $"Configuration '{key}' retrieved successfully"
                };
            }

            return new ConfigResult
            {
                Success = false,
                Key = key,
                Value = null,
                Message = $"Configuration key '{key}' not found"
            };
        }
    }

    /// <summary>
    /// Gets all configuration values.
    /// </summary>
    public AllConfigResult GetAllConfigurations()
    {
        lock (_lock)
        {
            return new AllConfigResult
            {
                Success = true,
                Configurations = new Dictionary<string, string>(_configurations),
                Count = _configurations.Count,
                Message = "All configurations retrieved successfully"
            };
        }
    }

    /// <summary>
    /// Updates a configuration value.
    /// </summary>
    public ConfigResult UpdateConfiguration(string key, string value)
    {
        lock (_lock)
        {
            var existed = _configurations.ContainsKey(key);
            var oldValue = existed ? _configurations[key] : null;
            
            _configurations[key] = value;

            return new ConfigResult
            {
                Success = true,
                Key = key,
                Value = value,
                OldValue = oldValue,
                Message = existed 
                    ? $"Configuration '{key}' updated from '{oldValue}' to '{value}'"
                    : $"Configuration '{key}' created with value '{value}'"
            };
        }
    }
}

/// <summary>
/// Result of a single configuration operation.
/// </summary>
public class ConfigResult
{
    public bool Success { get; set; }
    public string Key { get; set; } = string.Empty;
    public string? Value { get; set; }
    public string? OldValue { get; set; }
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Result of getting all configurations.
/// </summary>
public class AllConfigResult
{
    public bool Success { get; set; }
    public Dictionary<string, string> Configurations { get; set; } = new();
    public int Count { get; set; }
    public string Message { get; set; } = string.Empty;
}
