using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using Xunit;

namespace LabKeyEncrypter.Library;

public class UnitTest5
{
    [Fact]
    public void Decrypt_RoundTrip()
    {
        var plainText = "This is a secret";
        var password = "password";

        // Encrypt and then decrypt to verify round-trip works
        var encrypted = LabKeyEncrypter.Encrypt(plainText, password);
        var decrypted = LabKeyEncrypter.Decrypt(encrypted, password);

        Console.WriteLine(decrypted);

        Assert.Equal(plainText, decrypted);
    }

    [Fact]
    public void Decrypt_CorruptedPayload_ThrowsException()
    {
        var password = "password";
        var plainText = "This is a secret";
        
        // Encrypt legitimately first
        var encrypted = LabKeyEncrypter.Encrypt(plainText, password);
        var bytes = Convert.FromBase64String(encrypted);
        
        // Corrupt a byte in the ciphertext portion (after salt + nonce + tag = 16 + 12 + 16 = 44 bytes)
        if (bytes.Length > 45)
        {
            bytes[45] ^= 0xFF;
        }
        var corrupted = Convert.ToBase64String(bytes);

        // AES-GCM throws AuthenticationTagMismatchException when data is tampered
        Assert.Throws<System.Security.Cryptography.AuthenticationTagMismatchException>(() =>
        {
            var decrypted = LabKeyEncrypter.Decrypt(corrupted, password);
        });
    }

    [Fact]
    public void Decrypt_CorruptedBase64_ThrowsException()
    {
        var password = "password";

        var encrypted = "HFtK6d+wgtbXMuaIVNTE1kpvb/M4+sOBbRnlq8RomRrWwVECOi4sTamwL19nXXpENvu8UTKO2owy2jf6916lJA00==";

        Assert.Throws<System.FormatException>(() =>
        {
            var decrypted = LabKeyEncrypter.Decrypt(encrypted, password);
        });
    }
}
