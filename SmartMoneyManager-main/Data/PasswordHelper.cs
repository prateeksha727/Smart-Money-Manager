using System.Security.Cryptography;
using System.Text;

namespace SmartMoneyManager.Data
{
    public static class PasswordHelper
    {
        public static string Hash(string password)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password + "SMM_SALT_2024"));
            return Convert.ToHexString(bytes).ToLowerInvariant();
        }
        public static bool Verify(string password, string hash) => Hash(password) == hash;
    }
}
