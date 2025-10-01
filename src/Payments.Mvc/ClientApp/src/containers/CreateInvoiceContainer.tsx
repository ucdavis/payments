import * as React from 'react';

import 'isomorphic-fetch';

import { calculateDiscount } from '../helpers/calculations';

import { Account } from '../models/Account';
import { Coupon } from '../models/Coupon';
import { CreateInvoice } from '../models/CreateInvoice';
import { InvoiceAttachment } from '../models/InvoiceAttachment';
import { InvoiceCustomer } from '../models/InvoiceCustomer';
import { InvoiceDiscount } from '../models/InvoiceDiscount';
import { InvoiceItem } from '../models/InvoiceItem';
import { Team } from '../models/Team';

import AccountSelectControl from '../components/accountSelectControl';
import Alert from '../components/alert';
import AttachmentsControl from '../components/attachmentsControl';
import DueDateControl from '../components/dueDateControl';
import EditItemsTable from '../components/editItemsTable';
import InvoiceForm from '../components/invoiceForm';
import LoadingModal from '../components/loadingModal';
import MemoInput from '../components/memoInput';
import MultiCustomerControl from '../components/multiCustomerControl';
import SendModal from '../components/sendModal';

declare var antiForgeryToken: string;

interface IProps {
  accounts: Account[];
  coupons: Coupon[];
  team: Team;
}

interface IState {
  ids: number[] | undefined;
  accountId: number;
  attachments: InvoiceAttachment[];
  customers: InvoiceCustomer[];
  discount: InvoiceDiscount;
  dueDate: string;
  taxPercent: number;
  memo: string;
  items: InvoiceItem[];
  loading: boolean;
  errorMessage: string;
  modelErrors: string[];
  validate: boolean;
  invoiceType: string;

  isSendModalOpen: boolean;
}

export default class CreateInvoiceContainer extends React.Component<
  IProps,
  IState
> {
  private _formRef: HTMLFormElement;

  constructor(props: IProps) {
    super(props);

    const defaultAccount = props.accounts.find(a => a.isDefault);

    // Determine initial invoice type based on team's allowed invoice type
    let initialInvoiceType = 'CC'; // default
    if (props.team.allowedInvoiceType === 'CC') {
      initialInvoiceType = 'CC';
    } else if (props.team.allowedInvoiceType === 'Recharge') {
      initialInvoiceType = 'Recharge';
    } else if (props.team.allowedInvoiceType === 'Both') {
      initialInvoiceType = 'CC'; // default to CC for Both
    }

    this.state = {
      accountId: defaultAccount ? defaultAccount.id : 0,
      attachments: [],
      customers: [
        {
          address: '',
          company: '',
          email: '',
          name: ''
        }
      ],
      discount: {
        hasDiscount: false
      },
      dueDate: '',
      ids: undefined,
      items: [
        {
          amount: 0,
          description: '',
          id: 1,
          quantity: 0,
          taxExempt: false,
          total: 0
        }
      ],
      memo: '',
      taxPercent: 0,
      invoiceType: initialInvoiceType,

      errorMessage: '',
      modelErrors: [],
      loading: false,
      isSendModalOpen: false,
      validate: false
    };
  }

  public render() {
    const { accounts, coupons, team } = this.props;
    const {
      accountId,
      attachments,
      discount,
      dueDate,
      items,
      taxPercent,
      customers,
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
          <h1>Create Invoice for {team.name}</h1>
        </div>
        {this.renderInvoiceTypeToggle()}
        <div className='card-body invoice-customer'>
          <MultiCustomerControl
            customers={customers}
            onChange={c => this.updateProperty('customers', c)}
          />
          <div className='invalid-feedback'>Customer required.</div>
        </div>
        <div className='card-body invoice-items'>
          <h2>Invoice Items</h2>
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
          <h2>Memo</h2>
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
            <div className='col d-flex justify-content-end align-items-baseline'>
              <button className='btn-plain me-3' onClick={this.onSubmit}>
                Save and close
              </button>
              <button className='btn' onClick={this.openSendModal}>
                Send
              </button>
              {this.renderSendModal()}
            </div>
          </div>
        </div>
      </InvoiceForm>
    );
  }

  private renderInvoiceTypeToggle() {
    const { team } = this.props;
    const { invoiceType } = this.state;

    // Only show toggle if team allows both invoice types
    if (team.allowedInvoiceType !== 'Both') {
      return null;
    }

    const toggleContainerStyle: React.CSSProperties = {
      display: 'flex',
      justifyContent: 'flex-start',
      margin: '10px 0'
    };

    const toggleStyle: React.CSSProperties = {
      position: 'relative',
      display: 'flex',
      backgroundColor: '#f8f9fa',
      border: '2px solid #dee2e6',
      borderRadius: '25px',
      cursor: 'pointer',
      width: '280px',
      height: '50px',
      transition: 'all 0.3s ease',
      overflow: 'hidden'
    };

    const toggleOptionStyle: React.CSSProperties = {
      flex: 1,
      display: 'flex',
      alignItems: 'center',
      justifyContent: 'center',
      position: 'relative',
      zIndex: 2,
      transition: 'color 0.3s ease',
      fontWeight: 500,
      color: '#6c757d'
    };

    const activeOptionStyle: React.CSSProperties = {
      ...toggleOptionStyle,
      color: 'white !important' as any
    };

    const sliderStyle: React.CSSProperties = {
      position: 'absolute',
      top: 0,
      left: 0,
      width: '50%',
      height: '100%',
      background: '#1d4a83',
      borderRadius: '23px',
      transition: 'transform 0.3s ease',
      zIndex: 1,
      boxShadow: '0 2px 4px rgba(29,74,131,0.3)',
      transform:
        invoiceType === 'Recharge' ? 'translateX(100%)' : 'translateX(0%)'
    };

    return (
      <div className='card-body invoice-type'>
        <h2>Invoice Type</h2>
        <div className='form-group'>
          <div style={toggleContainerStyle}>
            <div
              style={toggleStyle}
              onClick={() =>
                this.updateProperty(
                  'invoiceType',
                  invoiceType === 'CC' ? 'Recharge' : 'CC'
                )
              }
            >
              <div
                style={
                  invoiceType === 'CC' ? activeOptionStyle : toggleOptionStyle
                }
              >
                <span
                  style={{ color: invoiceType === 'CC' ? 'white' : '#6c757d' }}
                >
                  Credit Card
                </span>
              </div>
              <div
                style={
                  invoiceType === 'Recharge'
                    ? activeOptionStyle
                    : toggleOptionStyle
                }
              >
                <span
                  style={{
                    color: invoiceType === 'Recharge' ? 'white' : '#6c757d'
                  }}
                >
                  Recharge
                </span>
              </div>
              <div style={sliderStyle}></div>
            </div>
          </div>
        </div>
      </div>
    );
  }

  private renderSendModal() {
    const { team } = this.props;
    const {
      attachments,
      dueDate,
      customers,
      discount,
      taxPercent,
      items,
      memo,
      isSendModalOpen
    } = this.state;

    if (!isSendModalOpen) {
      return null;
    }

    let customer: InvoiceCustomer;
    if (customers.length > 1) {
      customer = {
        address: '',
        company: '',
        email: 'Multiple Customers',
        name: 'Multiple Customers'
      };
    } else {
      customer = customers[0];
    }

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
        <strong className='me-3'>Error!</strong>
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

    const { slug } = this.props.team;
    const {
      accountId,
      attachments,
      discount,
      dueDate,
      customers,
      taxPercent,
      items,
      memo,
      invoiceType
    } = this.state;

    const calculatedDiscount = calculateDiscount(items, discount);

    // create submit object
    const invoice: CreateInvoice = {
      accountId,
      attachments,
      couponId: discount.couponId,
      customers,
      dueDate: new Date(dueDate),
      items,
      manualDiscount: calculatedDiscount,
      memo,
      taxPercent,
      type: invoiceType
    };

    // create url
    const url = `/${slug}/invoices/create`;

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
      this.setState({ ids: result.ids });
      return true;
    }

    this.setState({
      errorMessage: result.errorMessage,
      modelErrors: result.modelState.model.errors.map(e => e.errorMessage)
    });
    return false;
  };

  private sendInvoices = async (ccEmails: string) => {
    const { slug } = this.props.team;
    const { ids } = this.state;
    if (ids === undefined) {
      this.setState({
        errorMessage: 'Could not send email. Try to Save and Close instead.',
        modelErrors: []
      });
      return false;
    }

    // tslint:disable-next-line:prefer-for-of
    for (let i = 0; i < ids.length; i++) {
      const id = ids[i];
      // send invoice
      const url = `/${slug}/invoices/send/${id}`;

      // only send cc on the first email
      let body;
      if (i === 0) {
        body = JSON.stringify({
          ccEmails
        });
      } else {
        body = JSON.stringify({ ccEmails: '' });
      }

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
      if (!result.success) {
        this.setState({
          errorMessage: result.errorMessage,
          modelErrors: result.modelState.model.errors.map(e => e.errorMessage)
        });
        return false;
      }
    }

    return true;
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

    // send emails
    const sendResult = await this.sendInvoices(ccEmails);
    // a failure here means that the invoices are saved, just not all sent
    // send user back to invoices page with error message
    if (!sendResult) {
      // return to all invoices page
      window.location.pathname = `/${slug}/invoices`;
      return;
    }

    // return to all invoices page
    window.location.pathname = `/${slug}/invoices`;
  };

  private dismissErrorMessage = () => {
    this.setState({ errorMessage: '', modelErrors: [] });
  };
}
