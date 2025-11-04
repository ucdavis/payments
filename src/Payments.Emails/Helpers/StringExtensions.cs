
namespace Payments.Emails.Helpers
{
    public static class StringExtensions
    {
        public static string TruncateWithEllipsis(this string value, int maxLength = 100)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length < maxLength ? value : string.Format("{0}...", value.Substring(0, maxLength - 3));
        }
    }
}
