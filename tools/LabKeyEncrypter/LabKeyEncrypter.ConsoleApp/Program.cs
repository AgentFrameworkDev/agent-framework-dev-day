using LabKeyEncrypter.Library;
using System.Security.Cryptography;

return await RunAsync(args);

static async Task<int> RunAsync(string[] args)
{
    if (args is not [var operation, var filePath, var password])
    {
        Console.WriteLine("Usage: LabKeyEncrypter.ConsoleApp <encrypt|decrypt> <file> <password>");
        return 1;
    }

    try
    {
        var success = operation.ToLowerInvariant() switch
        {
            "encrypt" => await EncryptAsync(filePath, password),
            "decrypt" => await DecryptAsync(filePath, password),
            _ => throw new ArgumentException($"Invalid operation: {operation}")
        };

        Console.WriteLine(success ? "Operation completed successfully." : "Operation failed.");
        return success ? 0 : 1;
    }
    catch (Exception ex) when (ex is FileNotFoundException or AuthenticationTagMismatchException or ArgumentException)
    {
        Console.WriteLine($"Error: {ex.Message}");
        return 1;
    }
}

static async Task<bool> EncryptAsync(string filePath, string password)
{
    await LabKeyJsonFileValueEncrypter.EncryptJsonValuesAsync(filePath, password);
    return true;
}

static async Task<bool> DecryptAsync(string filePath, string password) =>
    await LabKeyJsonFileValueEncrypter.DecryptJsonValuesAsync(filePath, password);
