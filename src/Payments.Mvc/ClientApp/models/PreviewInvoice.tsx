import { Coupon } from './Coupon';
import { InvoiceAttachment } from './InvoiceAttachment';
import { InvoiceItem } from './InvoiceItem';

export interface PreviewInvoice {
    id: string;
    
    customerName: string;
    customerEmail: string;
    customerAddress: string;

    memo: string;
    coupon: Coupon;
    discount: number;
    taxPercent: number;

    items: InvoiceItem[];
    attachments: InvoiceAttachment[];

    dueDate?: Date;

    subTotal: number;
    taxAmount: number;
    total: number;

    teamName: string;
    teamContactEmail: string;
    teamContactPhone: string;
}