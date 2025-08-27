using System.Net.Mail;
using Common.Security;

namespace Common.Validation;

public static class Validation
{
    public static bool IsNonEmpty(string? s) => !string.IsNullOrWhiteSpace(s);

    public static bool IsValidEmail(string? email)
    {
        if (!IsNonEmpty(email)) return false;
        email = email!.Trim();
        if (email.Length > 254) return false;
        try
        {
            var addr = new MailAddress(email);
            // MailAddress normalizes; ensure it contains a dot in domain and no spaces
            return addr.Address == email && addr.Host.Contains('.');
        }
        catch
        {
            return false;
        }
    }

    // ISO 13616 + MOD 97-10 check. Accepts 15..34 chars, alphanumeric, starts with 2 letters + 2 digits
    public static bool IsValidIban(string? iban)
    {
        if (string.IsNullOrWhiteSpace(iban)) return false;
        var norm = Hashing.NormalizeIban(iban);
        if (norm.Length < 15 || norm.Length > 34) return false;
        if (norm.Length < 4) return false;
        if (!char.IsLetter(norm[0]) || !char.IsLetter(norm[1]) || !char.IsDigit(norm[2]) || !char.IsDigit(norm[3]))
            return false;
        // All alphanumeric already ensured by NormalizeIban; re-check to be safe
        if (norm.Any(c => !char.IsLetterOrDigit(c))) return false;

        // Rearrange
        var rearranged = norm.Substring(4) + norm.Substring(0, 4);
        // Convert to numeric string
        var sb = new System.Text.StringBuilder(rearranged.Length * 2);
        foreach (var ch in rearranged)
        {
            if (char.IsDigit(ch)) sb.Append(ch);
            else
            {
                int val = ch - 'A' + 10; // A=10..Z=35
                sb.Append(val.ToString());
            }
        }
        // Compute mod 97 iteratively to avoid BigInteger
        int mod = 0;
        foreach (var ch in sb.ToString())
        {
            mod = (mod * 10 + (ch - '0')) % 97;
        }
        return mod == 1;
    }
}
