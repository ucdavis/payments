import * as React from 'react';

import {
  calculateDiscount,
  calculateSubTotal,
  calculateTaxAmount,
  calculateTotal
} from '../helpers/calculations';

import { InvoiceItem } from '../models/InvoiceItem';
import { InvoiceDiscount } from '../models/InvoiceDiscount';
import { InvoiceCustomer } from '../models/InvoiceCustomer';
import { InvoiceAttachment } from '../models/InvoiceAttachment';
import { InvoiceRechargeItem } from '../models/InvoiceRechargeItem';
import { PreviewInvoice } from '../models/PreviewInvoice';
import { Team } from '../models/Team';
import { RechargeInvoiceModel } from '../containers/PayInvoiceContainer';

import Modal from './modal';
import PreviewFrame from './previewFrame';
import PreviewRechargeInvoiceContainer from '../containers/PreviewRechargeInvoiceContainer';

interface IProps {
  customer: InvoiceCustomer;
  memo: string;
  dueDate: string;

  items: InvoiceItem[];
  discount: InvoiceDiscount;
  taxPercent: number;

  attachments: InvoiceAttachment[];
  rechargeAccounts?: InvoiceRechargeItem[]; // Optional - only for recharge invoices

  team: Team;
  invoiceType?: string; // Optional for backward compatibility
  isModalOpen: boolean;

  onCancel: () => void;
  onSend: (ccEmails: string) => void;
}

interface IState {
  ccEmails: string;
}

export default class SendModal extends React.PureComponent<IProps, IState> {
  constructor(props: IProps) {
    super(props);

    this.state = {
      ccEmails: ''
    };
  }

  public render() {
    const { isModalOpen, onCancel } = this.props;

    return (
      <Modal
        dialogClassName='send-invoice-modal modal-lg'
        isOpen={isModalOpen}
        onBackdropClick={onCancel}
        onEscape={onCancel}
      >
        <div className='modal-header'>
          <div className='row flex-grow-1'>
            <div className='col-md-3' />
            <div className='col-md-6 d-flex justify-content-center align-items-center'>
              <h3 className='modal-title'>Send Invoice</h3>
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
        {this.renderSendBody()}
        {this.renderPreviewFrame()}
        <div className='modal-footer'>
          <div className='flex-grow-1 d-flex justify-content-between align-items-center'>
            <span>
              <i className='fas fa-info mx-3' /> This invoice can't be edited
              after it's sent.
            </span>
            <div className='d-flex align-items-baseline'>
              <button className='btn-outline-danger' onClick={onCancel}>
                Cancel
              </button>
              <button className='btn btn-primary' onClick={this.onSend}>
                Send Invoice
              </button>
            </div>
          </div>
        </div>
      </Modal>
    );
  }

  private renderPreviewFrame() {
    const {
      customer,
      items,
      discount,
      taxPercent,
      attachments,
      dueDate,
      memo,
      team,
      invoiceType,
      rechargeAccounts
    } = this.props;

    // Determine if this is a recharge invoice
    // Priority: explicit invoiceType prop > team.allowedInvoiceType
    const isRecharge = invoiceType
      ? invoiceType === 'Recharge'
      : team.allowedInvoiceType === 'Recharge';

    if (isRecharge) {
      // For recharge invoices, render the React preview component directly
      const subtotalCalc = calculateSubTotal(items);
      const totalCalc = calculateTotal(items, discount, taxPercent);

      // Filter to only include debit accounts for preview
      const debitAccounts = rechargeAccounts
        ? rechargeAccounts.filter(account => account.direction === 'Debit')
        : [];

      const rechargeModel: RechargeInvoiceModel = {
        id: 'PREVIEW-DRAFT',
        linkId: '',
        customerEmail: customer.email,
        memo,
        items: items.map(item => ({
          id: item.id,
          description: item.description,
          quantity: item.quantity,
          amount: item.amount,
          total: item.amount * item.quantity
        })),
        attachments: attachments.map((att, index) => ({
          id: index, // Use index as placeholder ID for preview
          fileName: att.fileName,
          contentType: att.contentType,
          size: att.size
        })),
        subtotal: subtotalCalc,
        total: totalCalc,
        dueDate: dueDate || null,
        paid: false,
        paidDate: null,
        team: {
          name: team.name,
          contactName: '',
          contactEmail: team.contactEmail,
          contactPhoneNumber: team.contactPhoneNumber
        },
        status: 'Draft',
        debitRechargeAccounts: debitAccounts
      };

      return (
        <div className='modal-body p-0'>
          <div style={{ minHeight: '60vh' }}>
            <PreviewRechargeInvoiceContainer invoice={rechargeModel} />
          </div>
        </div>
      );
    }

    // For credit card invoices, use the existing iframe approach
    const discountCalc = calculateDiscount(items, discount);
    const subtotalCalc = calculateSubTotal(items);
    const taxCalc = calculateTaxAmount(items, discount, taxPercent);
    const totalCalc = calculateTotal(items, discount, taxPercent);

    const model: PreviewInvoice = {
      id: 'PREVIEW-DRAFT',
      dueDate: dueDate ? new Date(dueDate) : undefined,
      memo,

      customerName: customer.name,
      customerAddress: customer.address,
      customerEmail: customer.email,

      items,
      coupon: discount.coupon,
      discount: discountCalc,
      taxPercent,
      attachments,

      subTotal: subtotalCalc,
      taxAmount: taxCalc,
      total: totalCalc,

      team: {
        name: team.name,
        contactEmail: team.contactEmail,
        contactPhoneNumber: team.contactPhoneNumber
      }
    };

    return (
      <div className='modal-body p-0'>
        <PreviewFrame invoice={model} />
      </div>
    );
  }

  private renderSendBody() {
    const email = this.props.customer.email;

    return (
      <div className='modal-body send-invoice-modal-body'>
        <div className='form-group row'>
          <label htmlFor='sendEmail' className='col-3 col-form-label text-end'>
            Send to
          </label>
          <div className='col-8'>
            <input
              type='text'
              className='form-control'
              name='sendEmail'
              readOnly={true}
              value={email}
            />
          </div>
        </div>
        <div className='form-group row'>
          <label htmlFor='cc' className='col-3 col-form-label text-end'>
            Cc
          </label>
          <div className='col-8'>
            <input
              type='text'
              className='form-control'
              name='cc'
              value={this.state.ccEmails}
              onChange={e => this.onCcEmailChange(e.target.value)}
            />
          </div>
        </div>
      </div>
    );
  }

  private onCcEmailChange = value => {
    this.setState({ ccEmails: value });
  };

  private onSend = () => {
    // TODO: Validate emails
    const ccEmails = this.state.ccEmails;

    this.props.onSend(ccEmails);
  };
}
