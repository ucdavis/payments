export interface Team {
  id: number;
  name: string;
  slug: string;
  allowedInvoiceType: string;
  canEditAccountOverride: boolean;

  contactEmail: string;
  contactPhoneNumber: string;
}
