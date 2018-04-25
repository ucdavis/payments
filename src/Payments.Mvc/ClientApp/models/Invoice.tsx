import { InvoiceItem } from './InvoiceItem';

export interface Invoice {
    id: number;
    customerName: string;
    customerEmail: string;
    customerAddress: string;
    discount: number;
    taxPercent: number;
    items: InvoiceItem[];
}