using System.Security.Cryptography;
using System.Text;

namespace QuantityMeasurementAppBusinessLayer.Services;

/// <summary>
/// UC18: Utility service that wraps BCrypt password hashing and SHA-256 hashing.
/// <para>
/// BCrypt is used for user passwords — it is slow by design (work factor = 11,
/// ~2048 rounds) which makes brute-force attacks impractical.
/// SHA-256 is provided as a fast general-purpose hashing utility.
/// </para>
/// </summary>
public class HashingService
{
    // ── BCrypt ────────────────────────────────────────────────────────

    /// <summary>
    /// Hashes <paramref name="plainText"/> using BCrypt with work factor 11.
    /// Always produces a different hash for the same input (salted).
    /// </summary>
    public string Hash(string plainText)
        => BCrypt.Net.BCrypt.HashPassword(plainText, workFactor: 11);

    /// <summary>
    /// Verifies <paramref name="plainText"/> against a BCrypt <paramref name="storedHash"/>.
    /// Returns true when the plain-text matches the hash.
    /// </summary>
    public bool Verify(string plainText, string storedHash)
        => BCrypt.Net.BCrypt.Verify(plainText, storedHash);

    // ── SHA-256 ───────────────────────────────────────────────────────

    /// <summary>
    /// Hashes <paramref name="input"/> with SHA-256 and returns a lowercase hex string.
    /// Deterministic — same input always produces the same output.
    /// </summary>
    public string HashSha256(string input)
    {
        byte[] inputBytes = Encoding.UTF8.GetBytes(input);
        byte[] hashBytes  = SHA256.HashData(inputBytes);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
}
