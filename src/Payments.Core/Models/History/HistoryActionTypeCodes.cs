using System;

namespace Payments.Core.Models.History
{
    public static class HistoryActionTypes
    {
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

        private static StringComparison _comparer = StringComparison.OrdinalIgnoreCase;

        public static IHistoryActionType GetHistoryActionType(string actionType)
        {
            if (string.Equals(CouponAddedByCustomer.TypeCode, actionType, _comparer))
            {
                return CouponAddedByCustomer;
            }

            if (string.Equals(CouponRemovedByCustomer.TypeCode, actionType, _comparer))
            {
                return CouponRemovedByCustomer;
            }

            if (string.Equals(InvoiceCreated.TypeCode, actionType, _comparer))
            {
                return InvoiceCreated;
            }

            if (string.Equals(InvoiceEdited.TypeCode, actionType, _comparer))
            {
                return InvoiceEdited;
            }

            if (string.Equals(InvoiceSent.TypeCode, actionType, _comparer))
            {
                return InvoiceSent;
            }

            if (string.Equals(InvoiceUnlocked.TypeCode, actionType, _comparer))
            {
                return InvoiceUnlocked;
            }

            if (string.Equals(InvoiceCancelled.TypeCode, actionType, _comparer))
            {
                return InvoiceCancelled;
            }

            if (string.Equals(InvoiceDeleted.TypeCode, actionType, _comparer))
            {
                return InvoiceDeleted;
            }

            if (string.Equals(PaymentCompleted.TypeCode, actionType, _comparer))
            {
                return PaymentCompleted;
            }

            if (string.Equals(PaymentFailed.TypeCode, actionType, _comparer))
            {
                return PaymentFailed;
            }

            if (string.Equals(MarkPaid.TypeCode, actionType, _comparer))
            {
                return MarkPaid;
            }

            return null;
        }
    }

    public interface IHistoryActionType
    {
        string TypeCode { get; }

        string IconClass { get; }

        string GetMessage(string data);
    }
}
