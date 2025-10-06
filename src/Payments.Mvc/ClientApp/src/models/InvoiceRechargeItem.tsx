export interface InvoiceRechargeItem {
  id: number;
  direction: 'Credit' | 'Debit';
  financialSegmentString: string;
  amount: number;
  percentage: number;
  //   enteredByKerb: string; //Might need these later
  //   enteredByName: string;
  //   approvedByKerb: string;
  //   approvedByName: string;
  notes: string;
}
