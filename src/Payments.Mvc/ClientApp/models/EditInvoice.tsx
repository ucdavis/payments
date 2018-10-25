import { InvoiceAttachment } from './InvoiceAttachment';
import { InvoiceCustomer } from './InvoiceCustomer';
import { InvoiceItem } from './InvoiceItem';

export interface EditInvoice {
    accountId: number;
    customer: InvoiceCustomer;
    memo: string;
    discount: number;
    dueDate?: Date;
    taxPercent: number;
    items: InvoiceItem[];
    attachments: InvoiceAttachment[];
}