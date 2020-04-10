using System;
using System.Security.Cryptography;
using System.Text;

namespace Payments.Mvc.Helpers
{
    public static class HashHelper
    {
        public static string GetSHA256Hash(this string input)
        {
            byte[] hash;
            using (var alg = SHA256.Create())
            {
                hash = alg.ComputeHash(Encoding.UTF8.GetBytes(input));
            }

            var sb = new StringBuilder();
            foreach (var b in hash)
            {
                sb.Append(b.ToString("X2"));
            }

            return sb.ToString();
        }
    }
}
