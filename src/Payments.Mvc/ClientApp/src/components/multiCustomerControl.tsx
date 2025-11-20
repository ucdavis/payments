import * as React from 'react';
import { InvoiceCustomer } from '../models/InvoiceCustomer';
import * as ArrayUtils from '../utils/array';

import CustomerControl from './customerControl';
import EditCustomerModal from './editCustomerModal';

const emailRegex = /^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$/;

const outlookRegex = /(?:"?([^"]*)"?\s)?(?:<?(.+@[^>]+)>?)/;

interface IProps {
  customers: InvoiceCustomer[];
  onChange: (value: InvoiceCustomer[]) => void;
}

interface IState {
  hasMultipleCustomers: boolean;
  multiCustomerInput: string;
  showEditModal: boolean;
  editModalCustomer: InvoiceCustomer | undefined;
}

export default class MultiCustomerControl extends React.Component<
  IProps,
  IState
> {
  constructor(props) {
    super(props);
    const { customers } = props;

    this.state = {
      editModalCustomer: undefined,
      hasMultipleCustomers: customers && customers.length > 1,
      multiCustomerInput: '',
      showEditModal: false
    };
  }

  public render() {
    return (
      <div className='multi-customer-control'>
        <div className='d-flex justify-content-between'>
          <h2>Customer Info</h2>
          {this.renderToggle()}
        </div>
        {this.renderContent()}
      </div>
    );
  }

  private renderToggle() {
    const { hasMultipleCustomers } = this.state;

    if (!hasMultipleCustomers) {
      return (
        <button
          className='btn btn-primary'
          type='button'
          onClick={this.enableMultiCustomer}
        >
          <i className='fas fa-plus me-3' />
          <i className='fas fa-users me-3' />
          Bill Multiple Customers
        </button>
      );
    }

    return (
      <button
        className='btn btn-primary'
        type='button'
        onClick={this.disableMultiCustomer}
      >
        <i className='fas fa-user me-2' />
        Bill Single Customer
      </button>
    );
  }

  private renderContent() {
    const { customers, onChange } = this.props;
    const { hasMultipleCustomers, multiCustomerInput } = this.state;

    if (!hasMultipleCustomers) {
      // use first or empty customer
      // TODO: there should be a better way to handle this
      const customer = customers.length
        ? customers[0]
        : { name: '', company: '', email: '', address: '' };

      return (
        <CustomerControl
          customer={customer}
          onChange={c => {
            onChange([c]);
          }}
        />
      );
    }

    return (
      <div className='form-group'>
        <div className='d-flex justify-content-between'>
          <label>Enter customer emails in bulk:</label>
        </div>

        <textarea
          rows={4}
          className='form-control mb-4'
          placeholder='user1@example.com; user2@example.com'
          onBlur={e => {
            this.updateCustomerList(e.target.value);
          }}
          onChange={e => {
            this.updateProperty('multiCustomerInput', e.target.value);
          }}
          value={multiCustomerInput}
        />

        <div className='email-badge-row'>
          {customers.map(c => this.renderCustomerTag(c))}
        </div>

        {this.renderEditModal()}
      </div>
    );
  }

  private renderCustomerTag(customer: InvoiceCustomer) {
    let text = customer.email;
    if (customer.name) {
      text = `${customer.name} <${customer.email}>`;
    }

    return (
      <div className='input-group mb-2' key={text}>
        <input
          type='text'
          className='form-control'
          value={text}
          readOnly={true}
        />
        <div className='input-group-text'>
          <button
            className='btn btn-primary'
            onClick={() => this.editCustomer(customer)}
          >
            <i className='far fa-fw fa-edit' />
          </button>
          <button
            className='btn btn-primary'
            onClick={() => this.removeCustomer(customer.email)}
          >
            <i className='fas fa-fw fa-times' />
          </button>
        </div>
      </div>
    );
  }

  private renderEditModal() {
    const { showEditModal, editModalCustomer } = this.state;

    if (!showEditModal) {
      return null;
    }

    return (
      <EditCustomerModal
        isModalOpen={showEditModal}
        customer={editModalCustomer}
        onCancel={this.closeEditModal}
        onConfirm={this.saveEditModal}
      />
    );
  }

  private enableMultiCustomer = () => {
    const { customers, onChange } = this.props;

    this.setState({
      hasMultipleCustomers: true
    });

    // clear empty customers
    if (!customers || !customers.length) {
      return;
    }
    const newCustomers = customers.filter(c => c.email);
    onChange(newCustomers);
  };

  private disableMultiCustomer = () => {
    this.setState({
      hasMultipleCustomers: false
    });
  };

  private updateProperty = (name: any, value: any) => {
    this.setState(({
      [name]: value
    } as unknown) as IState);
  };

  private updateCustomerList = (value: string) => {
    const { customers, onChange } = this.props;

    const emails = value
      .split(/[,;\r\n]+/)
      .filter(e => e.length)
      .map(e => e.trim());

    const validCustomers = [...customers];
    const invalidEmails: string[] = [];

    emails.forEach(e => {
      // check for outlook format first
      const outlookResult = outlookRegex.exec(e);
      if (outlookResult && outlookResult[1]) {
        validCustomers.push({
          address: '',
          company: '',
          email: outlookResult[2],
          name: outlookResult[1]
        });
        return;
      }

      if (emailRegex.test(e)) {
        validCustomers.push({
          address: '',
          company: '',
          email: e,
          name: ''
        });
        return;
      }

      invalidEmails.push(e);
    });

    // remove duplicates, map to customer
    const distinctCustomers = ArrayUtils.distinct(validCustomers, c => c.email);
    const sortedCustomers = distinctCustomers.sort((c1, c2) => {
      if (c1.email > c2.email) {
        return 1;
      }
      if (c1.email < c2.email) {
        return -1;
      }
      return 0;
    });

    onChange(sortedCustomers);
    this.setState({
      multiCustomerInput: invalidEmails.join('\n')
    });
  };

  private editCustomer = (customer: InvoiceCustomer) => {
    this.setState({
      editModalCustomer: customer,
      showEditModal: true
    });
  };

  private closeEditModal = () => {
    this.setState({
      showEditModal: false
    });
  };

  private saveEditModal = (customer: InvoiceCustomer) => {
    const { customers, onChange } = this.props;
    const { editModalCustomer } = this.state;

    // find customer being edited,
    const index = customers.findIndex(c => c.email === editModalCustomer.email);

    // then replace it
    const newCustomers = [...customers];
    newCustomers[index] = customer;

    onChange(newCustomers);

    this.setState({
      showEditModal: false
    });
  };

  private removeCustomer = (email: string) => {
    const { customers, onChange } = this.props;
    onChange(customers.filter(c => c.email !== email));
  };
}
