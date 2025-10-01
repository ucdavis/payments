import { InvoiceAttachment } from './InvoiceAttachment';
import { InvoiceCustomer } from './InvoiceCustomer';
import { InvoiceItem } from './InvoiceItem';

export interface EditInvoice {
  accountId: number;
  couponId: number;
  customer: InvoiceCustomer;
  memo: string;
  manualDiscount: number;
  dueDate?: Date;
  taxPercent: number;
  items: InvoiceItem[];
  attachments: InvoiceAttachment[];
  type: string;
}
