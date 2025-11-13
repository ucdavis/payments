using System;

namespace Payments.Core.Models.History
{
    public static class HistoryActionTypes
    {
        public static readonly IHistoryActionType DefaultActionType = new DefaultHistoryActionType();

        public static readonly IHistoryActionType CouponAddedByCustomer = new CouponAddedByCustomerHistoryActionType();

        public static readonly IHistoryActionType CouponRemovedByCustomer = new CouponRemovedByCustomerHistoryActionType();

        public static readonly IHistoryActionType InvoiceCreated = new InvoiceCreatedHistoryActionType();

        public static readonly IHistoryActionType InvoiceEdited = new InvoiceEditedHistoryActionType();

        public static readonly IHistoryActionType InvoiceSent = new InvoiceSentHistoryActionType();

        public static readonly IHistoryActionType InvoiceUnlocked = new InvoiceUnlockedHistoryActionType();

        public static readonly IHistoryActionType InvoiceCancelled = new InvoiceCancelledHistoryActionType();

        public static readonly IHistoryActionType InvoiceDeleted = new InvoiceDeletedHistoryActionType();

        public static readonly IHistoryActionType PaymentCompleted = new PaymentCompletedHistoryActionType();

        public static readonly IHistoryActionType PaymentFailed = new PaymentFailedHistoryActionType();

        public static readonly IHistoryActionType MarkPaid = new MarkPaidHistoryActionType();

        public static readonly IHistoryActionType SetBackToPaid = new SetBackToPaidHistoryActionType();

        public static readonly IHistoryActionType RefundRequested = new RefundRequestHistoryActionType();

        public static readonly IHistoryActionType RefundCancelled = new RefundCancelledHistoryActionType();

        public static readonly IHistoryActionType PaymentRefunded = new PaymentRefundedHistoryActionType();

        public static readonly IHistoryActionType RechargePaidByCustomer = new RechargePaidByCustomerHistoryActionType();

        public static readonly IHistoryActionType RechargeSentToFinancialApprovers = new RechargeSentToFinancialApproversHistoryActionType();

        public static readonly IHistoryActionType RechargeRejectedByFinancialApprover = new RechargeRejectedByFinancialApproverHistoryActionType();

        public static readonly IHistoryActionType RechargeRejected = new RechargeRejectedHistoryActionType();

        public static readonly IHistoryActionType RechargeApprovedByFinancialApprover = new RechargeApprovedByFinancialApproverHistoryActionType();

        private static StringComparison _comparer = StringComparison.OrdinalIgnoreCase;

        private static readonly IHistoryActionType[] AllTypes = new[]
        {
            CouponAddedByCustomer,
            CouponRemovedByCustomer,
            InvoiceCreated,
            InvoiceEdited,
            InvoiceSent,
            InvoiceUnlocked,
            InvoiceCancelled,
            InvoiceDeleted,
            PaymentCompleted,
            PaymentFailed,
            MarkPaid,
            SetBackToPaid,
            RefundRequested,
            PaymentRefunded,
            RefundCancelled,
            RechargePaidByCustomer,
            RechargeSentToFinancialApprovers,
            RechargeRejectedByFinancialApprover,
            RechargeRejected,
        };

        public static IHistoryActionType GetHistoryActionType(string actionType)
        {
            foreach (var historyType in AllTypes)
            {
                if (string.Equals(historyType.TypeCode, actionType, _comparer))
                {
                    return historyType;
                }
            }

            return DefaultActionType;
        }
    }

    public class DefaultHistoryActionType : IHistoryActionType
    {
        public string TypeCode => "";

        public string IconClass => "far fa-clock text-body";

        public string GetMessage(string data)
        {
            return $"Event: {data}";
        }

        public string GetDetails(string data)
        {
            return null;
        }
    }

    public interface IHistoryActionType
    {
        string TypeCode { get; }

        string IconClass { get; }

        string GetMessage(string data);

        string GetDetails(string data);

        public bool ShowDetails => false;
    }
}
