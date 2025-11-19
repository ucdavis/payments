import * as React from 'react';
import PreviewRechargeInvoiceContainer from '../containers/PreviewRechargeInvoiceContainer';
import { RechargeInvoiceModel } from '../containers/PayInvoiceContainer';

declare var model: RechargeInvoiceModel;

export const PreviewRechargeInvoicePage = () => {
  console.log('PreviewRechargeInvoicePage rendering', model);

  if (!model) {
    console.error('Model is not defined!');
    return <div>Error: Invoice data not available</div>;
  }

  return <PreviewRechargeInvoiceContainer invoice={model} />;
};
