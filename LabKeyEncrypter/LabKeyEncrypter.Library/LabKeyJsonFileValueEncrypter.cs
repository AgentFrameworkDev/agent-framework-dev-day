using System.Text.Json;
using System.Text.Json.Nodes;

namespace LabKeyEncrypter.Library;

/// <summary>
/// Encrypts and decrypts JSON file values using AES-256-GCM.
/// </summary>
public static class LabKeyJsonFileValueEncrypter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    /// <summary>
    /// Encrypts all top-level values in a JSON file and saves to a new file.
    /// </summary>
    /// <param name="filePath">Path to the JSON file to encrypt.</param>
    /// <param name="password">Password for encryption.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public static async Task EncryptJsonValuesAsync(string filePath, string password, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(filePath);
        ArgumentException.ThrowIfNullOrEmpty(password);

        var jsonString = await File.ReadAllTextAsync(filePath, cancellationToken);
        var jsonObject = JsonNode.Parse(jsonString)?.AsObject()
            ?? throw new JsonException("Invalid JSON: expected an object");

        foreach (var (key, value) in jsonObject.ToArray())
        {
            var plainText = value?.ToJsonString() ?? "null";
            jsonObject[key] = LabKeyEncrypter.Encrypt(plainText, password);
        }

        var encryptedFilePath = Path.ChangeExtension(filePath, null) + "_encrypted.json";
        await File.WriteAllTextAsync(encryptedFilePath, jsonObject.ToJsonString(JsonOptions), cancellationToken);
    }

    /// <summary>
    /// Encrypts all top-level values in a JSON file and saves to a new file (synchronous).
    /// </summary>
    public static void EncryptJsonValues(string filePath, string password) =>
        EncryptJsonValuesAsync(filePath, password).GetAwaiter().GetResult();

    /// <summary>
    /// Decrypts all top-level values in an encrypted JSON file and saves to a new file.
    /// </summary>
    /// <param name="filePath">Path to the encrypted JSON file.</param>
    /// <param name="password">Password for decryption.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if decryption was successful.</returns>
    public static async Task<bool> DecryptJsonValuesAsync(string filePath, string password, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(filePath);
        ArgumentException.ThrowIfNullOrEmpty(password);

        var jsonString = await File.ReadAllTextAsync(filePath, cancellationToken);
        var jsonObject = JsonNode.Parse(jsonString)?.AsObject()
            ?? throw new JsonException("Invalid JSON: expected an object");

        foreach (var (key, value) in jsonObject.ToArray())
        {
            var encrypted = value?.GetValue<string>()
                ?? throw new JsonException($"Expected string value for key '{key}'");
            var decrypted = LabKeyEncrypter.Decrypt(encrypted, password);
            jsonObject[key] = JsonNode.Parse(decrypted);
        }

        var decryptedFilePath = filePath.Replace("_encrypted.json", ".json", StringComparison.OrdinalIgnoreCase);
        await File.WriteAllTextAsync(decryptedFilePath, jsonObject.ToJsonString(JsonOptions), cancellationToken);
        return true;
    }

    /// <summary>
    /// Decrypts all top-level values in an encrypted JSON file (synchronous).
    /// </summary>
    public static bool DecryptJsonValues(string filePath, string password) =>
        DecryptJsonValuesAsync(filePath, password).GetAwaiter().GetResult();
}