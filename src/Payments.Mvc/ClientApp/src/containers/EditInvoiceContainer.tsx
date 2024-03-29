import * as React from 'react';

import { format } from 'date-fns';
import 'isomorphic-fetch';

import { calculateDiscount } from '../helpers/calculations';

import { Account } from '../models/Account';
import { Coupon } from '../models/Coupon';
import { EditInvoice } from '../models/EditInvoice';
import { InvoiceAttachment } from '../models/InvoiceAttachment';
import { InvoiceCustomer } from '../models/InvoiceCustomer';
import { InvoiceDiscount } from '../models/InvoiceDiscount';
import { InvoiceItem } from '../models/InvoiceItem';
import { Team } from '../models/Team';

import AccountSelectControl from '../components/accountSelectControl';
import Alert from '../components/alert';
import AttachmentsControl from '../components/attachmentsControl';
import CustomerControl from '../components/customerControl';
import DueDateControl from '../components/dueDateControl';
import EditItemsTable from '../components/editItemsTable';
import InvoiceForm from '../components/invoiceForm';
import LoadingModal from '../components/loadingModal';
import MemoInput from '../components/memoInput';
import SendModal from '../components/sendModal';

declare var antiForgeryToken: string;

interface IProps {
  accounts: Account[];
  coupons: Coupon[];
  id: number;
  invoice: EditInvoice;
  sent: boolean;
  team: Team;
}

interface IState {
  accountId: number;
  attachments: InvoiceAttachment[];
  customer: InvoiceCustomer;
  discount: InvoiceDiscount;
  dueDate: string;
  taxPercent: number;
  memo: string;
  items: InvoiceItem[];
  loading: boolean;
  errorMessage: string;
  modelErrors: string[];
  isSendModalOpen: boolean;
  validate: boolean;
}

export default class EditInvoiceContainer extends React.Component<
  IProps,
  IState
> {
  private _formRef: HTMLFormElement;

  constructor(props: IProps) {
    super(props);

    const { invoice } = this.props;

    // require at least one item
    const items = invoice.items || [];
    if (!items || items.length < 1) {
      items.push({
        amount: 0,
        description: '',
        id: 1,
        quantity: 0,
        taxExempt: false,
        total: 0
      });
    }

    // assign temp ids
    items.forEach((item, index) => {
      item.id = index + 1;
    });

    this.state = {
      accountId: invoice.accountId,
      attachments: invoice.attachments,
      customer: invoice.customer,
      discount: {
        couponId: invoice.couponId,
        hasDiscount: !!(invoice.couponId || invoice.manualDiscount),
        maunalAmount: invoice.manualDiscount
      },
      dueDate: invoice.dueDate ? format(invoice.dueDate, 'MM/DD/YYYY') : '',
      items,
      memo: invoice.memo,
      taxPercent: invoice.taxPercent || 0,

      errorMessage: '',
      modelErrors: [],
      loading: false,
      isSendModalOpen: false,
      validate: false
    };
  }

  public render() {
    const { id, sent, team, accounts, coupons } = this.props;
    const {
      accountId,
      attachments,
      customer,
      discount,
      dueDate,
      items,
      taxPercent,
      memo,
      loading,
      validate
    } = this.state;

    return (
      <InvoiceForm
        className='card'
        validate={validate}
        formRef={r => (this._formRef = r)}
      >
        <LoadingModal loading={loading} />
        <div className='card-header card-header-yellow'>
          <h1>
            Edit Invoice #{id} for {team.name}{' '}
          </h1>
        </div>
        <div className='card-body invoice-customer'>
          <h3>Customer Info</h3>
          <CustomerControl
            customer={customer}
            onChange={c => {
              this.updateProperty('customer', c);
            }}
          />
        </div>
        <div className='card-body invoice-items'>
          <h3>Invoice Items</h3>
          <EditItemsTable
            items={items}
            coupons={coupons}
            discount={discount}
            taxPercent={taxPercent}
            onItemsChange={v => this.updateProperty('items', v)}
            onDiscountChange={v => this.updateProperty('discount', v)}
            onTaxPercentChange={v => this.updateProperty('taxPercent', v)}
          />
        </div>
        <div className='card-body invoice-memo'>
          <h3>Memo</h3>
          <div className='form-group'>
            <MemoInput
              value={memo}
              onChange={v => this.updateProperty('memo', v)}
            />
          </div>
        </div>
        <div className='card-body invoice-billing'>
          <h2>Billing</h2>
          <div className='form-group'>
            <label>Due Date?</label>
            <DueDateControl
              value={dueDate}
              onChange={d => this.updateProperty('dueDate', d)}
            />
          </div>
          <div className='form-group'>
            <label>Income Account</label>
            <AccountSelectControl
              accounts={accounts}
              value={accountId}
              onChange={a => this.updateProperty('accountId', a)}
            />
          </div>
        </div>
        <div className='card-body invoice-attachments'>
          <h2>Attachments</h2>
          <AttachmentsControl
            attachments={attachments}
            onChange={v => this.updateProperty('attachments', v)}
          />
        </div>
        <div className='card-foot invoice-action'>
          <div className='row justify-content-between align-items-center'>
            <div className='col'>
              <button
                className='btn-plain color-unitrans'
                onClick={this.onCancel}
              >
                Cancel
              </button>
            </div>
            <div className='col d-flex justify-content-center align-items-end'>
              {this.renderError()}
            </div>
            <div className='col d-flex justify-content-end align-items-center'>
              <button className='btn-plain mr-3' onClick={this.onSubmit}>
                Save and close
              </button>
              {!sent && (
                <button className='btn' onClick={this.openSendModal}>
                  Send ...
                </button>
              )}
              {!sent && this.renderSendModal()}
            </div>
          </div>
        </div>
      </InvoiceForm>
    );
  }

  private renderSendModal() {
    const { team } = this.props;
    const {
      attachments,
      dueDate,
      customer,
      discount,
      taxPercent,
      items,
      memo,
      isSendModalOpen
    } = this.state;

    return (
      <SendModal
        isModalOpen={isSendModalOpen}
        customer={customer}
        memo={memo}
        dueDate={dueDate}
        taxPercent={taxPercent}
        discount={discount}
        items={items}
        attachments={attachments}
        team={team}
        onCancel={() => {
          this.setState({ isSendModalOpen: false });
        }}
        onSend={this.onSend}
      />
    );
  }

  private renderError() {
    const { errorMessage, modelErrors } = this.state;
    if (!errorMessage) {
      return null;
    }

    return (
      <Alert
        className='flex-grow-1 alert-danger'
        onDismiss={this.dismissErrorMessage}
      >
        <strong className='mr-3'>Error!</strong>
        <span>{errorMessage}</span>
        {modelErrors.map((e, i) => (
          <p key={i}>{e}</p>
        ))}
      </Alert>
    );
  }

  private updateProperty = (name: any, value: any) => {
    this.setState(({
      [name]: value
    } as unknown) as IState);
  };

  private onCancel = () => {
    window.history.go(-1);
  };

  private saveInvoice = async () => {
    // enable validation
    this.setState({ validate: true });

    // check validation
    const isValid = this._formRef.checkValidity();
    if (!isValid) {
      return false;
    }

    const { id } = this.props;
    const { slug } = this.props.team;
    const {
      accountId,
      attachments,
      customer,
      discount,
      dueDate,
      taxPercent,
      items,
      memo
    } = this.state;

    const calculatedDiscount = calculateDiscount(items, discount);

    // create submit object
    const invoice: EditInvoice = {
      accountId,
      attachments,
      couponId: discount.couponId,
      customer,
      dueDate: new Date(dueDate),
      items,
      manualDiscount: calculatedDiscount,
      memo,
      taxPercent
    };

    // create save url
    const url = `/${slug}/invoices/edit/${id}`;

    // fetch
    const response = await fetch(url, {
      body: JSON.stringify(invoice),
      credentials: 'same-origin',
      headers: new Headers({
        'Content-Type': 'application/json',
        RequestVerificationToken: antiForgeryToken
      }),
      method: 'POST'
    });

    const result = await response.json();
    if (result.success) {
      return true;
    }

    this.setState({
      errorMessage: result.errorMessage,
      modelErrors: result.modelState.model.errors.map(e => e.errorMessage)
    });
    return false;
  };

  private sendInvoice = async (ccEmails: string) => {
    // send invoice
    const { id } = this.props;
    const { slug } = this.props.team;

    const url = `/${slug}/invoices/send/${id}`;

    const body = JSON.stringify({
      ccEmails
    });

    const response = await fetch(url, {
      credentials: 'same-origin',
      headers: new Headers({
        'Content-Type': 'application/json',
        RequestVerificationToken: antiForgeryToken
      }),
      body,
      method: 'POST'
    });

    const result = await response.json();
    if (result.success) {
      return true;
    }

    this.setState({
      errorMessage: result.errorMessage,
      modelErrors: result.modelState.model.errors.map(e => e.errorMessage)
    });
    return false;
  };

  private onSubmit = async () => {
    const { slug } = this.props.team;
    this.setState({ loading: true });

    // save invoice
    const saveResult = await this.saveInvoice();
    if (!saveResult) {
      this.setState({ loading: false });
      return;
    }

    // return to all invoices page
    window.location.pathname = `/${slug}/invoices`;
  };

  private openSendModal = () => {
    this.setState({
      isSendModalOpen: true
    });
  };

  private onSend = async (ccEmails: string) => {
    const { slug } = this.props.team;
    this.setState({ loading: true });

    // save invoice
    const saveResult = await this.saveInvoice();
    if (!saveResult) {
      this.setState({ loading: false });
      return;
    }

    const sendResult = await this.sendInvoice(ccEmails);
    if (!sendResult) {
      this.setState({ loading: false });
      return;
    }

    // return to all invoices page
    window.location.pathname = `/${slug}/invoices`;
  };

  private dismissErrorMessage = () => {
    this.setState({ errorMessage: '', modelErrors: [] });
  };
}
