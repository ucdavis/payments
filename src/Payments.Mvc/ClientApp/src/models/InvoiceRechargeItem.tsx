// Validation models matching C# AccountValidationModel
export interface AccountValidationModel {
  isValid: boolean;
  chartString: string;
  messages: string[];
  warnings: Array<{ key: string; value: string }>;
  details: Array<{ key: string; value: string }>;
}

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
  // Validation state
  validationResult?: AccountValidationModel;
  isValidating?: boolean;
  hasValidationError?: boolean;
  skipNextValidation?: boolean; // Flag to prevent validation loops when updating from validation results
}
