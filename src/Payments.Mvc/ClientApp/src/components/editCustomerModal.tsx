import * as React from 'react';

import { InvoiceCustomer } from '../models/InvoiceCustomer';

import CustomerControl from './customerControl';
import Modal from './modal';

interface IProps {
  customer: InvoiceCustomer;
  isModalOpen: boolean;

  onCancel: () => void;
  onConfirm: (customer: InvoiceCustomer) => void;
}

interface IState {
  customer: InvoiceCustomer;
}

export default class EditCustomerModal extends React.Component<IProps, IState> {
  constructor(props) {
    super(props);

    this.state = {
      customer: props.customer
    };
  }

  public render() {
    const { isModalOpen, onCancel } = this.props;
    const { customer } = this.state;

    return (
      <Modal
        dialogClassName='edit-customer-modal modal-lg'
        isOpen={isModalOpen}
        onBackdropClick={onCancel}
        onEscape={onCancel}
      >
        <div className='modal-header'>
          <div className='row flex-grow-1'>
            <div className='col-md-3' />
            <div className='col-md-6 d-flex justify-content-center align-items-center'>
              <h3 className='modal-title'>Edit Customer</h3>
            </div>
            <div className='col-md-3 d-flex justify-content-end align-items-center'>
              <button
                type='button'
                className='btn-close m-1'
                onClick={onCancel}
              >
                <span aria-hidden='true'></span>
              </button>
            </div>
          </div>
        </div>
        <div className='modal-body'>
          <CustomerControl customer={customer} onChange={this.onChange} />
        </div>
        <div className='modal-footer'>
          <div className='flex-grow-1 d-flex justify-content-between align-items-center'>
            <button className='btn btn-outline-danger me-3' onClick={onCancel}>
              Cancel
            </button>
            <button className='btn btn-primary' onClick={this.onConfirm}>
              Save
            </button>
          </div>
        </div>
      </Modal>
    );
  }

  private onChange = (customer: InvoiceCustomer) => {
    this.setState({
      customer
    });
  };

  private onConfirm = () => {
    this.props.onConfirm(this.state.customer);
  };
}
