import * as React from 'react';

import { InvoiceCustomer } from '../models/InvoiceCustomer';

interface IProps {
  invoiceType: string;
  customers?: InvoiceCustomer[];
  customer?: InvoiceCustomer;
}

export default class EmailWarning extends React.Component<IProps> {
  public render() {
    const { invoiceType, customers, customer } = this.props;

    // Only show warning for Recharge invoices
    if (invoiceType !== 'Recharge') {
      return null;
    }

    let hasNonUCDavisEmail = false;

    // Check for multiple customers (create invoice)
    if (customers) {
      hasNonUCDavisEmail = customers.some(
        c => c.email && !c.email.toLowerCase().endsWith('ucdavis.edu')
      );
    }
    // Check for single customer (edit invoice)
    else if (customer) {
      hasNonUCDavisEmail =
        !!customer.email &&
        !customer.email.toLowerCase().endsWith('ucdavis.edu');
    }

    if (!hasNonUCDavisEmail) {
      return null;
    }

    return (
      <div className='alert alert-warning mt-2' role='alert'>
        <i className='fas fa-exclamation-triangle me-2'></i>
        <strong>Warning:</strong> Recharge invoices should typically use a
        ucdavis.edu email address.
      </div>
    );
  }
}
