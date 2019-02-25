import { InvoiceAttachment } from './InvoiceAttachment';
import { InvoiceCustomer } from './InvoiceCustomer';
import { InvoiceItem } from './InvoiceItem';

export interface CreateInvoice {
    accountId: number;
    couponId: number;
    customers: InvoiceCustomer[];
    memo: string;
    manualDiscount: number;
    dueDate?: Date;
    taxPercent: number;
    items: InvoiceItem[];
    attachments: InvoiceAttachment[];
}
