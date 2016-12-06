using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Payments.Tests.Helpers
{
    public static class UniqueDbName
    {
        public static string GetName()
        {
            return string.Format("db_{0}", Guid.NewGuid());
        }
    }
}
