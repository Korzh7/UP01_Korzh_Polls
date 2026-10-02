using System.Security.Cryptography;
using System.Text;

namespace UP01_Korzh_Polls.Models;

public static class SecurityHelper
{
    // Шаг 7. Реализация хеширования паролей (SHA1 по методичке)
    public static string GetHash(string password)
    {
        if (string.IsNullOrEmpty(password)) return string.Empty;
        using var hash = SHA1.Create();
        return string.Concat(hash.ComputeHash(Encoding.UTF8.GetBytes(password)).Select(x => x.ToString("X2")));
    }
}
