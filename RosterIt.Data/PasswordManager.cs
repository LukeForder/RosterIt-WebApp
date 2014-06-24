using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace RosterIt.Models
{
    public static class PasswordManager
    {
        public static byte[] GenerateSalt(int length)
        {
            var rng = RNGCryptoServiceProvider.Create();

            byte[] data = new byte[length];

            rng.GetBytes(data);

            return data;
        }

        public static string Encode(IEnumerable<byte> data)
        {
            return Convert.ToBase64String(data.ToArray(), Base64FormattingOptions.None);
        }

        public static byte[] Decode(string encodedBase64)
        {
            return Convert.FromBase64String(encodedBase64);
        }

        public static byte[] ComputeHash(string password, byte[] salt)
        {
            var hashAlgorithm = SHA512Managed.Create();

            var passwordBytes = Encoding.UTF8.GetBytes(password);

            var saltedPassword = salt.Concat(passwordBytes).ToArray();

            var hash = hashAlgorithm.ComputeHash(saltedPassword);

            return hash;
        }
    }
}
