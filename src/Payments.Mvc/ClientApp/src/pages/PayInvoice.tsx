import * as React from 'react';
import PayInvoiceContainer, {
  RechargeInvoiceModel
} from '../containers/PayInvoiceContainer';

declare var model: RechargeInvoiceModel;

export const PayInvoicePage = () => <PayInvoiceContainer invoice={model} />;
