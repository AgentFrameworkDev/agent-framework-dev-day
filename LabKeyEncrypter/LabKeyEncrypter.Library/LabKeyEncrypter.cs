using System.Security.Cryptography;
using System.Text;

namespace LabKeyEncrypter.Library;

/// <summary>
/// AES-256-GCM encryption with PBKDF2-SHA256 key derivation.
/// Compliant with OWASP cryptographic storage guidelines.
/// </summary>
public static class LabKeyEncrypter
{
    private const int SaltSize = 16;       // 128 bit
    private const int KeySize = 32;        // 256 bit
    private const int NonceSize = 12;      // 96 bit (recommended for AES-GCM)
    private const int TagSize = 16;        // 128 bit (AES-GCM authentication tag)
    private const int HeaderSize = SaltSize + NonceSize + TagSize;
    private const int Iterations = 600_000; // OWASP recommended minimum for PBKDF2-SHA256

    /// <summary>
    /// Encrypts plaintext using AES-256-GCM with PBKDF2-SHA256 key derivation.
    /// </summary>
    /// <param name="plainText">The text to encrypt.</param>
    /// <param name="password">The password to derive the encryption key from.</param>
    /// <returns>Base64-encoded string containing salt + nonce + tag + ciphertext.</returns>
    public static string Encrypt(string plainText, string password)
    {
        ArgumentNullException.ThrowIfNull(plainText);
        ArgumentException.ThrowIfNullOrEmpty(password);

        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var result = new byte[HeaderSize + plainBytes.Length];

        // Generate salt and nonce directly into result buffer
        var salt = result.AsSpan(0, SaltSize);
        var nonce = result.AsSpan(SaltSize, NonceSize);
        var tag = result.AsSpan(SaltSize + NonceSize, TagSize);
        var ciphertext = result.AsSpan(HeaderSize);

        RandomNumberGenerator.Fill(salt);
        RandomNumberGenerator.Fill(nonce);

        // Derive key using stack allocation for small buffers
        Span<byte> key = stackalloc byte[KeySize];
        Rfc2898DeriveBytes.Pbkdf2(password, salt, key, Iterations, HashAlgorithmName.SHA256);

        using var aesGcm = new AesGcm(key, TagSize);
        aesGcm.Encrypt(nonce, plainBytes, ciphertext, tag);

        // Clear the key from memory
        CryptographicOperations.ZeroMemory(key);

        return Convert.ToBase64String(result);
    }

    /// <summary>
    /// Decrypts ciphertext that was encrypted with the Encrypt method.
    /// </summary>
    /// <param name="cipherText">Base64-encoded string containing salt + nonce + tag + ciphertext.</param>
    /// <param name="password">The password used for encryption.</param>
    /// <returns>The decrypted plaintext.</returns>
    /// <exception cref="AuthenticationTagMismatchException">Thrown if authentication fails (tampered data or wrong password).</exception>
    public static string Decrypt(string cipherText, string password)
    {
        ArgumentException.ThrowIfNullOrEmpty(cipherText);
        ArgumentException.ThrowIfNullOrEmpty(password);

        var fullCipher = Convert.FromBase64String(cipherText);

        if (fullCipher.Length < HeaderSize)
            throw new ArgumentException("Invalid ciphertext: too short", nameof(cipherText));

        // Extract components using Span slicing: salt (16) + nonce (12) + tag (16) + ciphertext
        ReadOnlySpan<byte> data = fullCipher;
        var salt = data[..SaltSize];
        var nonce = data[SaltSize..(SaltSize + NonceSize)];
        var tag = data[(SaltSize + NonceSize)..HeaderSize];
        var cipherBytes = data[HeaderSize..];

        // Derive key using stack allocation
        Span<byte> key = stackalloc byte[KeySize];
        Rfc2898DeriveBytes.Pbkdf2(password, salt, key, Iterations, HashAlgorithmName.SHA256);

        var plainBytes = new byte[cipherBytes.Length];

        using var aesGcm = new AesGcm(key, TagSize);
        aesGcm.Decrypt(nonce, cipherBytes, tag, plainBytes);

        // Clear the key from memory
        CryptographicOperations.ZeroMemory(key);

        return Encoding.UTF8.GetString(plainBytes);
    }
}