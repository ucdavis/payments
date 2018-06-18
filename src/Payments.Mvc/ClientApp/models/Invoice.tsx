import { InvoiceCustomer } from './InvoiceCustomer';
import { InvoiceItem } from './InvoiceItem';

export interface Invoice {
    customer: InvoiceCustomer;
    memo: string;
    discount: number;
    tax: number;
    items: InvoiceItem[];
}