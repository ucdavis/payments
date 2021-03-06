using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Payments.Core.Extensions
{
    public static class StringExtensions
    {
        public static string SafeToUpper(this string value)
        {
            if (value == null)
            {
                return null;
            }

            return value.ToUpper(CultureInfo.InvariantCulture);
        }

        public static string SafeTruncate(this string value, int max)
        {
            if (string.IsNullOrWhiteSpace(value) || value.Length <= max)
            {
                return value;
            }

            if (max <= 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            return value.Substring(0, max);
        }

        public static string SafeRegexRemove(this string value, string regEx = @"[^0-9a-zA-Z\.\-\' ]+")
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
            
            try
            {
                return Regex.Replace(value, regEx, string.Empty);
            }
            catch (Exception)
            {
                return value;
            }

            
        }
    }
}
