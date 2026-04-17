using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace QuantityMeasurementAppBusinessLayer.Services;

/// <summary>
/// UC18: AES-256 symmetric encryption and decryption utility.
/// <para>
/// A fresh random IV (initialisation vector) is generated for every
/// <see cref="Encrypt"/> call and prepended to the cipher-text so that
/// <see cref="Decrypt"/> can extract it without storing it separately.
/// This means encrypting the same plain-text twice produces different
/// cipher-texts — which is the correct behaviour.
/// </para>
/// <para>
/// Configuration key: <c>Encryption:Key</c> in appsettings.json.
/// Must be at least 32 UTF-8 characters (padded/truncated to exactly 32 bytes
/// for AES-256).
/// </para>
/// </summary>
public class EncryptionService
{
    // AES-256 requires exactly 32 bytes
    private readonly byte[] _encryptionKey;

    public EncryptionService(IConfiguration configuration)
    {
        string keyValue = configuration["Encryption:Key"]
            ?? throw new InvalidOperationException("Encryption:Key is missing from configuration.");

        // Pad or truncate to exactly 32 bytes
        _encryptionKey = new byte[32];
        byte[] keyBytes = Encoding.UTF8.GetBytes(keyValue);
        Array.Copy(keyBytes, _encryptionKey, Math.Min(keyBytes.Length, _encryptionKey.Length));
    }

    // ── Encrypt ───────────────────────────────────────────────────────

    /// <summary>
    /// Encrypts <paramref name="plainText"/> with AES-256-CBC and returns
    /// a Base64 string of (IV + cipherText).
    /// </summary>
    public string Encrypt(string plainText)
    {
        using Aes aes = Aes.Create();
        aes.Key = _encryptionKey;
        aes.GenerateIV();                    // random IV for every call
        byte[] iv = aes.IV;

        using var ms        = new MemoryStream();
        ms.Write(iv, 0, iv.Length);          // prepend IV so Decrypt can extract it

        using var encryptor = aes.CreateEncryptor();
        using var cs        = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
        using var sw        = new StreamWriter(cs);
        sw.Write(plainText);

        return Convert.ToBase64String(ms.ToArray());
    }

    // ── Decrypt ───────────────────────────────────────────────────────

    /// <summary>
    /// Decrypts a Base64 cipher-text produced by <see cref="Encrypt"/> back
    /// to the original plain-text string.
    /// </summary>
    public string Decrypt(string cipherText)
    {
        byte[] allBytes = Convert.FromBase64String(cipherText);

        using Aes aes = Aes.Create();
        aes.Key = _encryptionKey;

        // First block-size bytes are the IV
        int ivLength   = aes.BlockSize / 8;
        byte[] iv      = new byte[ivLength];
        Array.Copy(allBytes, 0, iv, 0, ivLength);
        aes.IV = iv;

        // Remaining bytes are the actual cipher text
        byte[] cipherBytes = new byte[allBytes.Length - ivLength];
        Array.Copy(allBytes, ivLength, cipherBytes, 0, cipherBytes.Length);

        using var decryptor = aes.CreateDecryptor();
        using var ms        = new MemoryStream(cipherBytes);
        using var cs        = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        using var sr        = new StreamReader(cs);
        return sr.ReadToEnd();
    }
}
