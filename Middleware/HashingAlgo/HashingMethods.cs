using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Middleware.HashingAlgo
{
    public class HashingMethods
    {
        private const int SaltSize = 16;  // 128-bit salt
        private const int HashSize = 32;  // 256-bit hash
        private const int Iterations = 10000; // Number of iterations

        /// <summary>
        /// Generates a salted hash for the given password.
        /// </summary>
        public static string HashPassword(string password)
        {
            try
            {
                byte[] salt = new byte[SaltSize];
                using (var rng = new RNGCryptoServiceProvider())
                {
                    rng.GetBytes(salt); // Generate random salt
                }

                using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations))
                {
                    byte[] hash = pbkdf2.GetBytes(HashSize);
                    byte[] hashBytes = new byte[SaltSize + HashSize];

                    Array.Copy(salt, 0, hashBytes, 0, SaltSize);
                    Array.Copy(hash, 0, hashBytes, SaltSize, HashSize);

                    return Convert.ToBase64String(hashBytes);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error generating hashed password.", ex);
            }
        }

        /// <summary>
        /// Verifies a password against a stored hash.
        /// </summary>
        public static bool VerifyPassword(string enteredPassword, string storedHash)
        {
            try
            {
                byte[] hashBytes = Convert.FromBase64String(storedHash);
                byte[] salt = new byte[SaltSize];
                byte[] storedPasswordHash = new byte[HashSize];

                Array.Copy(hashBytes, 0, salt, 0, SaltSize);
                Array.Copy(hashBytes, SaltSize, storedPasswordHash, 0, HashSize);

                using (var pbkdf2 = new Rfc2898DeriveBytes(enteredPassword, salt, Iterations))
                {
                    byte[] enteredPasswordHash = pbkdf2.GetBytes(HashSize);
                    return CompareHashes(enteredPasswordHash, storedPasswordHash);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error verifying password.", ex);
            }
        }

        /// <summary>
        /// Compares two hashes in a secure way.
        /// </summary>
        private static bool CompareHashes(byte[] hash1, byte[] hash2)
        {
            if (hash1.Length != hash2.Length)
                return false;

            for (int i = 0; i < hash1.Length; i++)
            {
                if (hash1[i] != hash2[i])
                    return false;
            }

            return true;
        }
    }
}
