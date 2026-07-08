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

        /// <summary>
        /// Splits a full name into first and last name parts using only spaces as delimiters.
        /// For one-word names, the word is treated as the last name; for names with more than two words, the final word is the last name and the rest is the first name.
        /// Null input returns null parts, and empty or space-only input returns empty parts.
        /// </summary>
        public static (string FirstName, string LastName) ParseFirstAndLastName(this string value)
        {
            if (value == null)
            {
                return (null, null);
            }

            if (value == string.Empty)
            {
                return (string.Empty, string.Empty);
            }

            var names = value.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (names.Length == 0)
            {
                return (string.Empty, string.Empty);
            }

            if (names.Length == 1)
            {
                return (string.Empty, names[0]);
            }

            return (string.Join(" ", names, 0, names.Length - 1), names[names.Length - 1]);
        }
    }
}
