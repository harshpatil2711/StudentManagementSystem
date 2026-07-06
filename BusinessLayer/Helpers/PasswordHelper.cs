using System;
using System.Security.Cryptography;
using System.Text;

namespace BusinessLayer.Helpers
{
    public static class PasswordHelper
    {
        private const int SALT_SIZE = 16;
        private const int HASH_SIZE = 32;

        public static string HashPassword(string password)
        {
            byte[] salt = new byte[SALT_SIZE];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            byte[] hash;
            using (var hmac = new HMACSHA256(salt))
            {
                hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            }

            return Convert.ToBase64String(salt) + ":" + Convert.ToBase64String(hash);
        }

        public static bool VerifyPassword(string password, string storedHash)
        {
            if (string.IsNullOrEmpty(storedHash) || !storedHash.Contains(":"))
                return false;

            var parts = storedHash.Split(':');
            if (parts.Length != 2)
                return false;

            byte[] salt;
            byte[] stored;
            try
            {
                salt = Convert.FromBase64String(parts[0]);
                stored = Convert.FromBase64String(parts[1]);
            }
            catch
            {
                return false;
            }

            using (var hmac = new HMACSHA256(salt))
            {
                byte[] computed = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(stored) == Convert.ToBase64String(computed);
            }
        }
    }
}
