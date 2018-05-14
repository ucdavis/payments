using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Payments.Mvc.Helpers
{
    public class InvoiceKeyHelper
    {
        public static string GetUniqueKey(int maxSize)
        {
            // all uppercase characters
            // ignore similar characters (0/O) (1/I) (2/Z)
            var chars = "ABCDEFGHJKLMNPQRSTUVWXY3456789".ToCharArray();
            var data = new byte[1];
            using (var crypto = new RNGCryptoServiceProvider())
            {
                crypto.GetNonZeroBytes(data);
                data = new byte[maxSize];
                crypto.GetNonZeroBytes(data);
            }
            var result = new StringBuilder(maxSize);
            foreach (var b in data)
            {
                result.Append(chars[b % (chars.Length)]);
            }
            return result.ToString();
        }
    }
}
