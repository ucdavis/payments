import { InvoiceCustomer } from './InvoiceCustomer';
import { InvoiceItem } from './InvoiceItem';

export interface Invoice {
    accountId: number;
    customer: InvoiceCustomer;
    memo: string;
    discount: number;
    dueDate?: Date;
    tax: number;
    items: InvoiceItem[];
}