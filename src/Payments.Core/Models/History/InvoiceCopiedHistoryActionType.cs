using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Payments.Core.Models.History
{
    public class InvoiceCopiedHistoryActionType : IHistoryActionType
    {
        public string TypeCode => "invoice-copied";

        public string IconClass => "far fa-copy text-body";

        public string GetMessage(string data)
        {
            return data;
        }

        public string GetDetails(string data)
        {
            return null;
        }
    }
}
