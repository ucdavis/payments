export interface InvoiceItem {
    id: number;
    description: string;
    quantity: number;
    amount: number;
    taxExempt: boolean;

    total: number;
}