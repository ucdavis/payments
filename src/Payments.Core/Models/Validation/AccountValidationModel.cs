using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Payments.Core.Models.Validation
{
    public class AccountValidationModel
    {
        public bool IsValid { get; set; } = true;

        //Have warnings?
        
        public string Message
        {
            get
            {
                if (Messages.Count <= 0)
                {
                    return string.Empty;
                }

                return string.Join(" ", Messages);
            }
        }

        public List<string> Messages { get; set; } = new List<string>();
    }
}