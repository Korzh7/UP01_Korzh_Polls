using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace UP01_Korzh_Polls.Models;

/// <summary>
/// Вспомогательный класс безопасности для криптографического хеширования паролей
/// </summary>
public static class SecurityHelper
{
    /// <summary>
    /// Вычисление SHA-1 хеша строки пароля с возвратом в шестнадцатеричном представлении
    /// </summary>
    /// <param name="password">Исходный пароль пользователя</param>
    /// <returns>Шестнадцатеричная строка хеша SHA-1</returns>
    public static string GetHash(string password)
    {
        if (string.IsNullOrEmpty(password)) return string.Empty;
        using var hash = SHA1.Create();
        return string.Concat(hash.ComputeHash(Encoding.UTF8.GetBytes(password)).Select(x => x.ToString("X2")));
    }
}
