using System;
using System.Collections.Generic;

namespace Payments.Mvc.Models.CyberSource
{
    /* There are more codes, but we most likely won't support them
     * These strings are such that they match the values in CyberSource XML Reports
     * */

    public static class CyberSourceCardTypes
    {
        private static readonly Dictionary<string, string> LookupCodes;
        private static readonly Dictionary<string, string> LookupNames;

        static CyberSourceCardTypes()
        {
            LookupCodes = new Dictionary<string, string>()
            {
                {"visa"            , "001"},
                {"mastercard"      , "002"},
                {"american express", "003"},
                {"discover"        , "004"},
                {"diners club"     , "005"},
                {"carte blanche"   , "006"},
                {"jcb"             , "007"}
            };

            LookupNames = new Dictionary<string, string>()
            {
                {"001", "visa"            },
                {"002", "mastercard"      },
                {"003", "american express"},
                {"004", "discover"        },
                {"005", "diners club"     },
                {"006", "carte blanche"   },
                {"007", "jcb"             }
            };
        }

        /// <summary>
        /// Get Card Code from Card Name
        /// </summary>
        /// <param name="cardName"></param>
        /// <returns></returns>
        public static string GetCode(string cardName)
        {
            return LookupCodes[cardName.ToLower()];
        }

        /// <summary>
        /// Get Card Name from Card Code
        /// </summary>
        /// <param name="cardCode"></param>
        /// <returns></returns>
        public static string GetName(string cardCode)
        {
            return LookupNames[cardCode];
        }

        public static string GetIconClass(string cardCode)
        {
            if (cardCode == "001") return "fab fa-cc-visa";

            if (cardCode == "002") return "fab fa-cc-mastercard";

            if (cardCode == "003") return "fab fa-cc-amex";

            if (cardCode == "004") return "fab fa-cc-discover";

            if (cardCode == "005") return "fab fa-cc-diners-club";

            if (cardCode == "006") return "fas fa-credit-card";

            if (cardCode == "007") return "fab fa-cc-jcb";

            return "fas fa-credit-card";
        }

        public static string FormatCardNumber(string number, string cardCode)
        {
            if (string.IsNullOrWhiteSpace(number))
            {
                return "";
            }

            // most common 4-4-4-4 style
            string[] parts =
            {
                number.Substring(0, 4),
                number.Substring(4, 4),
                number.Substring(8, 4),
                number.Substring(12, 4),
            };

            // amex 4-6-5 style
            if (cardCode == "003")
            {
                parts = new[] {
                    number.Substring(0, 4),
                    number.Substring(4, 6),
                    number.Substring(10, 5)
                };
            }

            // card blanche 4-6-4 style
            if (cardCode == "006")
            {
                parts = new[] {
                    number.Substring(0, 4),
                    number.Substring(4, 6),
                    number.Substring(10, 4)
                };
            }

            return string.Join(' ', parts);
        }
    }
}
