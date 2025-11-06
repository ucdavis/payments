import * as React from 'react';
import FinancialApproveInvoiceContainer, {
  RechargeInvoiceModel
} from '../containers/FinancialApproveInvoiceContainer';

declare var model: RechargeInvoiceModel;

export const FinancialApproveInvoicePage = () => (
  <FinancialApproveInvoiceContainer invoice={model} />
);
