using System;
using System.Collections.Generic;
using System.Text;

namespace Payments.Core.Models.History
{
    public static class HistoryActionTypes
    {
        public static readonly IHistoryActionType InvoiceCreated = new InvoiceCreatedHistoryActionType();

        public static readonly IHistoryActionType InvoiceEdited = new InvoiceEditedHistoryActionType();

        public static readonly IHistoryActionType InvoiceSent = new InvoiceSentHistoryActionType();

        public static readonly IHistoryActionType InvoiceUnlocked = new InvoiceUnlockedHistoryActionType();

        public static readonly IHistoryActionType InvoiceClosed = new InvoiceClosedHistoryActionType();
        public static readonly IHistoryActionType InvoiceDeleted = new InvoiceDeletedHistoryActionType();

        public static readonly IHistoryActionType PaymentCompleted = new PaymentCompletedHistoryActionType();

        public static readonly IHistoryActionType PaymentFailed = new PaymentFailedHistoryActionType();

        public static IHistoryActionType GetHistoryActionType(string actionType)
        {
            if (string.Equals(InvoiceCreated.TypeCode, actionType, StringComparison.OrdinalIgnoreCase))
            {
                return InvoiceCreated;
            }

            if (string.Equals(InvoiceEdited.TypeCode, actionType, StringComparison.OrdinalIgnoreCase))
            {
                return InvoiceEdited;
            }

            if (string.Equals(InvoiceSent.TypeCode, actionType, StringComparison.OrdinalIgnoreCase))
            {
                return InvoiceSent;
            }

            if (string.Equals(InvoiceUnlocked.TypeCode, actionType, StringComparison.OrdinalIgnoreCase))
            {
                return InvoiceUnlocked;
            }

            if (string.Equals(InvoiceClosed.TypeCode, actionType, StringComparison.OrdinalIgnoreCase))
            if (string.Equals(InvoiceDeleted.TypeCode, actionType, StringComparison.OrdinalIgnoreCase))
            {
                return InvoiceDeleted;
            }

            if (string.Equals(PaymentCompleted.TypeCode, actionType, StringComparison.OrdinalIgnoreCase))
            {
                return PaymentCompleted;
            }

            if (string.Equals(PaymentFailed.TypeCode, actionType, StringComparison.OrdinalIgnoreCase))
            {
                return PaymentFailed;
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
