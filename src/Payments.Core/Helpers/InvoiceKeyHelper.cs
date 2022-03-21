using System.Security.Cryptography;
using System.Text;

namespace Payments.Core.Helpers
{
    public class InvoiceKeyHelper
    {
        public static readonly int KeyLength = 10;

        public static string GetUniqueKey()
        {
            // all uppercase characters
            // ignore similar characters (0/O) (1/I) (2/Z)
            var chars = "ABCDEFGHJKLMNPQRSTUVWXY3456789".ToCharArray();
            var data = new byte[1];
            using (var crypto = RandomNumberGenerator.Create())
            {
                crypto.GetNonZeroBytes(data);
                data = new byte[KeyLength];
                crypto.GetNonZeroBytes(data);
            }
            var result = new StringBuilder(KeyLength);
            foreach (var b in data)
            {
                result.Append(chars[b % (chars.Length)]);
            }
            return result.ToString();
        }
    }
}
