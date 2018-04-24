import { InvoiceItem } from './InvoiceItem';

export interface Invoice {
    id: number;
    items: InvoiceItem[];
}