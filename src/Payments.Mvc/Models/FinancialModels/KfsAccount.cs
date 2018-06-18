using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Payments.Core.Extensions;

namespace Payments.Mvc.Models.FinancialModels
{
    public class KfsAccount
    {
        public string chartOfAccountsCode { get; set; }
        public string accountNumber { get; set; }
        public string accountName { get; set; }
        public DateTime? accountExpirationDate { get; set; }
        public bool? closed { get; set; }

        public string subFundGroupTypeCode { get; set; } 
        
        public string subFundGroupName { get; set; }

        public bool IsValidIncomeAccount
        {
            get
            {
                if (closed.HasValue && closed.Value)
                {
                    return false;
                }
                if (string.IsNullOrWhiteSpace(subFundGroupTypeCode))
                {
                    return false;
                }

                if (subFundGroupTypeCode == "1") //Agency Accounts
                {
                    return true;
                }
                if (subFundGroupTypeCode == "4") //Sales and Service of Teaching Hospital
                {
                    return true;
                }
                if (subFundGroupTypeCode.SafeToUpper() == "M") //Self Supporting Activities(Other Sources
                {
                    return true;
                }

                if (subFundGroupTypeCode.SafeToUpper() == "Y") //Sales and Service Educational Activities
                {
                    return true;
                }


                return false;
            }
        }

        public static implicit operator KfsAccount(string v)
        {
            throw new NotImplementedException();
        }
    }
}
