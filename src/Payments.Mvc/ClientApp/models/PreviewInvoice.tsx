import { Coupon } from './Coupon';
import { InvoiceAttachment } from './InvoiceAttachment';
import { InvoiceCustomer } from './InvoiceCustomer';
import { InvoiceItem } from './InvoiceItem';

export interface PreviewInvoice {
    coupon: Coupon;
    customerEmail: string;
    memo: string;
    discount: number;
    dueDate?: Date;
    taxPercent: number;
    items: InvoiceItem[];
    attachments: InvoiceAttachment[];
}