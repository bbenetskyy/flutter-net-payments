using System.Security.Cryptography;
using System.Text;

namespace AuthService.Domain.Services;

public static class Hashing
{
    // Pepper з appsettings: Security:HashPepper
    public static (string hashHex, string saltHex) HashSecret(string value, string? saltHex, string pepper)
    {
        var salt = saltHex is null ? RandomNumberGenerator.GetBytes(16) : Convert.FromHexString(saltHex);
        var data = Encoding.UTF8.GetBytes(value.Trim());
        var pepperBytes = Encoding.UTF8.GetBytes(pepper);

        // IBAN нормалізуємо до upper без пробілів
        // DOB формат YYYY-MM-DD

        using var sha = SHA256.Create();
        var toHash = data
            .Concat(salt)
            .Concat(pepperBytes)
            .ToArray();
        var hash = sha.ComputeHash(toHash);
        return (Convert.ToHexString(hash), Convert.ToHexString(salt));
    }

    public static string NormalizeIban(string iban) =>
        new string(iban.ToUpperInvariant().Where(char.IsLetterOrDigit).ToArray());
}
