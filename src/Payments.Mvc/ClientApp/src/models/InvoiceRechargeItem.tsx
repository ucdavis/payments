export interface RechargeAccountItem {
  id: number;
  invoiceId: number;
  direction: 'Credit' | 'Debit';
  financialSegmentString: string;
  amount: number;
  percentage: number;
  enteredByKerb: string;
  enteredByName: string;
  approvedByKerb: string;
  approvedByName: string;
  notes: string;
}
