export interface Team {
  id: number;
  name: string;
  slug: string;
  allowedInvoiceType: string;
  canEditCreditCardRechargeAccount: boolean;

  contactEmail: string;
  contactPhoneNumber: string;
}
