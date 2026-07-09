using Newtonsoft.Json;
using System.Text;

namespace Payments.Core.Models.History
{
    public class AccountOverrideChangedHistoryActionType : IHistoryActionType
    {
        public string TypeCode => "account-override-changed";

        public string IconClass => "fas fa-file-invoice-dollar text-info";

        public bool ShowDetails => true;

        public string GetMessage(string data)
        {
            var d = DeserializeData(data);
            if (d == null)
            {
                return "Income account override was updated.";
            }

            return d.Action switch
            {
                ChangeActions.Added => "Income account override was added.",
                ChangeActions.Removed => "Income account override was removed.",
                ChangeActions.Changed => "Income account override was changed.",
                ChangeActions.NotCopiedDueToPermissions => "Income account override was not copied because the user does not have permission.",
                _ => "Income account override was updated."
            };
        }

        public string GetDetails(string data)
        {
            var d = DeserializeData(data);
            if (d == null)
            {
                return null;
            }

            var sb = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(d.PreviousChartString))
            {
                sb.Append($"&nbsp;&nbsp;Previous: {d.PreviousChartString}<br/>");
            }

            if (!string.IsNullOrWhiteSpace(d.NewChartString))
            {
                sb.Append($"&nbsp;&nbsp;New: {d.NewChartString}<br/>");
            }

            return sb.ToString().TrimEnd();
        }

        public DataType DeserializeData(string data)
        {
            try
            {
                return JsonConvert.DeserializeObject<DataType>(data);
            }
            catch
            {
                return null;
            }
        }

        public string SerializeData(DataType data)
        {
            return JsonConvert.SerializeObject(data);
        }

        public static class ChangeActions
        {
            public const string Added = "Added";
            public const string Removed = "Removed";
            public const string Changed = "Changed";
            public const string NotCopiedDueToPermissions = "NotCopiedDueToPermissions";
        }

        public class DataType
        {
            public string Action { get; set; }

            public string PreviousChartString { get; set; }

            public string NewChartString { get; set; }
        }
    }
}