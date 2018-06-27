import { InvoiceCustomer } from './InvoiceCustomer';
import { InvoiceItem } from './InvoiceItem';

export interface Invoice {
    accountId: number;
    customer: InvoiceCustomer;
    memo: string;
    discount: number;
    tax: number;
    items: InvoiceItem[];
}