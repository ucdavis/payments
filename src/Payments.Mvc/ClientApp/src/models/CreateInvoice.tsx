import { InvoiceAttachment } from './InvoiceAttachment';
import { InvoiceCustomer } from './InvoiceCustomer';
import { InvoiceItem } from './InvoiceItem';
import { InvoiceRechargeItem } from './InvoiceRechargeItem';

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
  type: string;
  rechargeAccounts: InvoiceRechargeItem[];
}
