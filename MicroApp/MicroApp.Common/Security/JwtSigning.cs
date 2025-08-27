using System.Security.Cryptography;
using System.Text;

namespace Common.Security;

public static class JwtSigning
{
    /// <summary>
    /// Produces normalized key bytes for HMAC JWT signing/validation.
    /// Steps:
    /// 1. Accepts raw configuration value (Jwt:Key).
    /// 2. Try Base64 decode; if it fails, use UTF-8 bytes.
    /// 3. If resulting bytes are shorter than 32 bytes (256 bits), derive 32 bytes via SHA-256.
    /// </summary>
    public static byte[] GetKeyBytes(string? rawKey)
    {
        if (string.IsNullOrWhiteSpace(rawKey))
            throw new InvalidOperationException("JWT signing key (Jwt:Key) is not configured.");

        var trimmed = rawKey.Trim();

        byte[] keyBytes;
        try
        {
            keyBytes = Convert.FromBase64String(trimmed);
        }
        catch
        {
            keyBytes = Encoding.UTF8.GetBytes(trimmed);
        }

        if (keyBytes.Length < 32)
        {
            keyBytes = SHA256.HashData(keyBytes);
        }

        return keyBytes;
    }
}
