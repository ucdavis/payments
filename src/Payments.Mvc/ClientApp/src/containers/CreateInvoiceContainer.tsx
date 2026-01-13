import * as React from 'react';

import 'isomorphic-fetch';

import { calculateDiscount, calculateTotal } from '../helpers/calculations';

import { Account } from '../models/Account';
import { Coupon } from '../models/Coupon';
import { CreateInvoice } from '../models/CreateInvoice';
import { InvoiceAttachment } from '../models/InvoiceAttachment';
import { InvoiceCustomer } from '../models/InvoiceCustomer';
import { InvoiceDiscount } from '../models/InvoiceDiscount';
import { InvoiceItem } from '../models/InvoiceItem';
import { InvoiceRechargeItem } from '../models/InvoiceRechargeItem';
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
import RechargeAccountsControl from '../components/rechargeAccountsControl';
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
  rechargeAccounts: InvoiceRechargeItem[];
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
  private _rechargeAccountsRef: RechargeAccountsControl;

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
      rechargeAccounts: [],

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
      validate,
      invoiceType,
      rechargeAccounts
    } = this.state;

    return (
      <InvoiceForm
        className='card'
        validate={validate}
        formRef={r => { this._formRef = r; }}
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
            invoiceType={invoiceType}
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
        {invoiceType === 'Recharge' && (
          <div className='card-body invoice-recharge-accounts'>
            <RechargeAccountsControl
              ref={r => { this._rechargeAccountsRef = r; }}
              rechargeAccounts={rechargeAccounts}
              invoiceTotal={calculateTotal(items, discount, taxPercent)}
              onChange={v => this.updateProperty('rechargeAccounts', v)}
              showCreditAccounts={true}
            />
          </div>
        )}
        <div className='card-body invoice-billing'>
          <h2>Billing</h2>
          <div className='form-group'>
            <label>Due Date?</label>
            <DueDateControl
              value={dueDate}
              onChange={d => this.updateProperty('dueDate', d)}
            />
          </div>
          {invoiceType !== 'Recharge' && (
            <div className='form-group'>
              <label>Income Account</label>
              <AccountSelectControl
                accounts={accounts}
                value={accountId}
                onChange={a => this.updateProperty('accountId', a)}
              />
            </div>
          )}
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
                className='btn btn-outline-danger'
                onClick={this.onCancel}
              >
                Cancel
              </button>
            </div>
            <div className='col d-flex justify-content-center align-items-end'>
              {this.renderError()}
            </div>
            <div className='col d-flex justify-content-end align-items-baseline'>
              <button
                className='btn btn-outline-primary me-2'
                onClick={this.onSubmit}
              >
                Save and close
              </button>
              <button className='btn btn-primary' onClick={this.openSendModal}>
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

    return (
      <div className='card-body invoice-type'>
        <h2>Invoice Type</h2>
        <div className='form-group'>
          <div className='invoice-type-toggle-container'>
            <div
              className='invoice-type-toggle'
              onClick={() =>
                this.updateProperty(
                  'invoiceType',
                  invoiceType === 'CC' ? 'Recharge' : 'CC'
                )
              }
            >
              <div
                className={`invoice-type-toggle-option ${
                  invoiceType === 'CC' ? 'active' : ''
                }`}
              >
                <span>
                  <i
                    className='fas fa-credit-card me-2'
                    title='Credit Card'
                  ></i>
                  Credit Card
                </span>
              </div>
              <div
                className={`invoice-type-toggle-option ${
                  invoiceType === 'Recharge' ? 'active' : ''
                }`}
              >
                <span>
                  <i className='fas fa-registered me-2' title='Registered'></i>
                  Recharge
                </span>
              </div>
              <div
                className={`invoice-type-toggle-slider ${
                  invoiceType === 'Recharge' ? 'slide-right' : 'slide-left'
                }`}
              ></div>
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
      isSendModalOpen,
      invoiceType,
      rechargeAccounts
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
        invoiceType={invoiceType}
        rechargeAccounts={rechargeAccounts}
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

    // check for chart string validation errors if this is a recharge invoice
    if (this.state.invoiceType === 'Recharge' && this._rechargeAccountsRef) {
      const hasChartStringErrors = this._rechargeAccountsRef.hasValidationErrors();
      if (hasChartStringErrors) {
        this.setState({
          errorMessage:
            'Please fix all chart string validation errors before saving.',
          modelErrors: []
        });
        return false;
      }
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
      invoiceType,
      rechargeAccounts
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
      rechargeAccounts,
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

    // Extract model errors from ModelState
    let modelErrors: string[] = [];
    if (result.modelState) {
      // ModelState is an object where keys are property names and values are arrays of error messages
      Object.keys(result.modelState).forEach(key => {
        const errors = result.modelState[key];
        if (Array.isArray(errors)) {
          errors.forEach(error => {
            if (typeof error === 'string') {
              modelErrors.push(error);
            } else if (error && error.errorMessage) {
              modelErrors.push(error.errorMessage);
            }
          });
        }
      });
    }

    this.setState({
      errorMessage: result.errorMessage,
      modelErrors: modelErrors
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
        // Extract model errors from ModelState
        let modelErrors: string[] = [];
        if (result.modelState) {
          // ModelState is an object where keys are property names and values are arrays of error messages
          Object.keys(result.modelState).forEach(key => {
            const errors = result.modelState[key];
            if (Array.isArray(errors)) {
              errors.forEach(error => {
                if (typeof error === 'string') {
                  modelErrors.push(error);
                } else if (error && error.errorMessage) {
                  modelErrors.push(error.errorMessage);
                }
              });
            }
          });
        }

        this.setState({
          errorMessage: result.errorMessage,
          modelErrors: modelErrors
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
    // enable validation
    this.setState({ validate: true });

    // check validation
    const isValid = this._formRef.checkValidity();
    if (!isValid) {
      return;
    }

    // check for chart string validation errors if this is a recharge invoice
    if (this.state.invoiceType === 'Recharge' && this._rechargeAccountsRef) {
      const hasChartStringErrors = this._rechargeAccountsRef.hasValidationErrors();
      if (hasChartStringErrors) {
        this.setState({
          errorMessage:
            'Please fix all chart string validation errors before sending.',
          modelErrors: []
        });
        return;
      }
    }

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
