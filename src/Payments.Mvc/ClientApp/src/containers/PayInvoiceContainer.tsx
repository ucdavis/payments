import * as React from 'react';
import RechargeAccountsControl from '../components/rechargeAccountsControl';
import { InvoiceRechargeItem } from '../models/InvoiceRechargeItem';

declare var antiForgeryToken: string;

export interface RechargeInvoiceModel {
  id: string; // formatted invoice number
  linkId: string;
  customerEmail: string;
  memo: string;
  items: InvoiceLineItem[];
  attachments: InvoiceAttachmentModel[];
  subtotal: number;
  total: number;
  dueDate: string | null;
  paid: boolean;
  paidDate: string | null;
  team: InvoiceTeam;
  status: string;
  debitRechargeAccounts: InvoiceRechargeItem[];
}

export interface InvoiceLineItem {
  id: number;
  description: string;
  quantity: number;
  amount: number;
  total: number;
}

export interface InvoiceAttachmentModel {
  id: number;
  fileName: string;
  contentType: string;
  size: number;
}

export interface InvoiceTeam {
  name: string;
  contactName: string;
  contactEmail: string;
  contactPhoneNumber: string;
}

interface IProps {
  invoice: RechargeInvoiceModel;
}

interface IState {
  rechargeAccounts: InvoiceRechargeItem[];
  isValid: boolean;
  isSaving: boolean;
  errorMessage: string;
}

export default class PayInvoiceContainer extends React.Component<
  IProps,
  IState
> {
  private _rechargeAccountsRef: RechargeAccountsControl;

  constructor(props: IProps) {
    super(props);

    this.state = {
      rechargeAccounts: props.invoice.debitRechargeAccounts || [],
      isValid: false,
      isSaving: false,
      errorMessage: ''
    };
  }

  componentDidMount() {
    // Validate initial state
    this.validateForm();
  }

  public render() {
    const { invoice } = this.props;
    const { rechargeAccounts, isValid, isSaving, errorMessage } = this.state;
    const canEdit = invoice.status === 'Sent';

    return (
      <div className='card pay-card'>
        <div className='card-gradient-header-bleed'>
          <div className='card-gradient-header'></div>
        </div>
        <div>
          {/* Top Info */}
          <div className='pay-top'>
            <h1 className='mb-0'>Invoice #{invoice.id}</h1>
            <h2 className=''>From {invoice.team.name}</h2>
            <br />
          </div>

          {/* Payment Action Area */}
          <div className='pay-action'>
            {!invoice.paid && (
              <>
                <span className='pay-action-total'>
                  ${invoice.total.toFixed(2)}
                </span>

                {invoice.dueDate && (
                  <span className='pay-action-date secondary-font'>
                    Due{' '}
                    {new Date(invoice.dueDate).toLocaleDateString('en-US', {
                      year: 'numeric',
                      month: 'long',
                      day: 'numeric'
                    })}
                  </span>
                )}

                {errorMessage && (
                  <div className='alert alert-danger mt-3' role='alert'>
                    {errorMessage}
                  </div>
                )}

                <div style={{ alignContent: 'center' }}>
                  <button
                    type='button'
                    className='btn-gold btn-lg pay-now-button'
                    onClick={this.handleSubmit}
                    disabled={!isValid || isSaving}
                  >
                    {isSaving ? (
                      <>
                        <i className='fas fa-spinner fa-spin me-3' />
                        Processing...
                      </>
                    ) : (
                      <>
                        <i className='fas fa-check me-3' aria-hidden='true' />
                        Submit for Approval
                      </>
                    )}
                  </button>
                </div>
              </>
            )}

            {invoice.paid && (
              <>
                <h1>Invoice Paid</h1>
                {invoice.paidDate && (
                  <h2>
                    ${invoice.total.toFixed(2)} USD paid{' '}
                    {new Date(invoice.paidDate).toLocaleDateString('en-US', {
                      year: 'numeric',
                      month: 'long',
                      day: 'numeric'
                    })}
                  </h2>
                )}
              </>
            )}
          </div>

          {/* Summary Section */}
          <div className='pay-description'>
            {invoice.memo && (
              <>
                <h3 className='secondary-font'>Memo</h3>
                <div className='pay-memo'>
                  <p>{invoice.memo}</p>
                </div>
              </>
            )}

            {/* Line Items Table */}
            <table className='table pay-table'>
              <thead>
                <tr>
                  <th>Description</th>
                  <th>Quantity</th>
                  <th>Unit Price</th>
                  <th>Amount</th>
                </tr>
              </thead>
              <tbody>
                {invoice.items.map(item => (
                  <tr key={item.id}>
                    <td>{item.description}</td>
                    <td>{item.quantity}</td>
                    <td>${item.amount.toFixed(2)}</td>
                    <td>${item.total.toFixed(2)}</td>
                  </tr>
                ))}
                <tr>
                  <td></td>
                  <td>Total:</td>
                  <td></td>
                  <td>${invoice.total.toFixed(2)}</td>
                </tr>
              </tbody>
            </table>
          </div>

          {/* Recharge Accounts - Only editable if status is Sent */}
          {canEdit && (
            <div className='card-body invoice-recharge-accounts'>
              <h3>Debit Chart Strings</h3>
              <p className='text-muted'>
                Please provide at least one valid debit chart string. The total
                amount must match the invoice total.
              </p>
              <RechargeAccountsControl
                ref={r => (this._rechargeAccountsRef = r)}
                rechargeAccounts={rechargeAccounts}
                invoiceTotal={invoice.total}
                onChange={this.handleRechargeAccountsChange}
              />
            </div>
          )}

          {/* Display existing recharge accounts if not editable */}
          {!canEdit && rechargeAccounts.length > 0 && (
            <div className='card-body'>
              <h3>Debit Chart Strings</h3>
              <table className='table'>
                <thead>
                  <tr>
                    <th>Chart String</th>
                    <th>Amount</th>
                    <th>Notes</th>
                  </tr>
                </thead>
                <tbody>
                  {rechargeAccounts.map(account => (
                    <tr key={account.id}>
                      <td>{account.financialSegmentString}</td>
                      <td>${account.amount.toFixed(2)}</td>
                      <td>{account.notes}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}

          {/* Attachments */}
          {invoice.attachments.length > 0 && (
            <div className='pay-attachments'>
              <h3 className='secondary-font'>Attachments</h3>
              {invoice.attachments.map(attachment => {
                const href = `/file/${invoice.linkId}/${attachment.id}`;
                const iconClass = this.getFileIcon(attachment.contentType);

                return (
                  <a
                    key={attachment.id}
                    className='pay-attachment'
                    target='_blank'
                    rel='noreferrer'
                    href={href}
                  >
                    <p className='col-6'>
                      <i className={`${iconClass} fa-fw`}></i>
                      {attachment.fileName}
                    </p>
                    <span className='col-5 text-end'>
                      {this.getSizeText(attachment.size)}
                    </span>
                    <div className='col-1 text-end'>
                      <i className='fas fa-download'></i>
                    </div>
                  </a>
                );
              })}
            </div>
          )}

          {/* Download PDF */}
          <div className='pay-download'>
            <img src='/media/download.svg' alt='download icon' />
            {invoice.paid ? (
              <a
                href={`/pdf/receipt/${invoice.linkId}`}
                className='btn-inverse'
                download
              >
                Download PDF
              </a>
            ) : (
              <a
                href={`/pdf/${invoice.linkId}`}
                className='btn-inverse'
                download
              >
                Download PDF
              </a>
            )}
          </div>

          {/* Footer */}
          <div className='pay-footer'>
            <h3 className='secondary-font'>Questions?</h3>
            <div className='pay-footer-contact'>
              {invoice.team.contactName && (
                <p>
                  <strong>Contact:</strong> {invoice.team.contactName}
                </p>
              )}
              {invoice.team.contactEmail && (
                <p>
                  <strong>Email:</strong>{' '}
                  <a href={`mailto:${invoice.team.contactEmail}`}>
                    {invoice.team.contactEmail}
                  </a>
                </p>
              )}
              {invoice.team.contactPhoneNumber && (
                <p>
                  <strong>Phone:</strong> {invoice.team.contactPhoneNumber}
                </p>
              )}
            </div>
          </div>
        </div>
      </div>
    );
  }

  private handleRechargeAccountsChange = (accounts: InvoiceRechargeItem[]) => {
    this.setState({ rechargeAccounts: accounts }, () => {
      this.validateForm();
    });
  };

  private validateForm = () => {
    const { rechargeAccounts } = this.state;
    const { invoice } = this.props;

    // Check if we have at least one valid debit chart string
    const validAccounts = rechargeAccounts.filter(
      account =>
        account.direction === 'Debit' &&
        account.financialSegmentString &&
        account.financialSegmentString.trim() !== '' &&
        account.amount > 0 &&
        account.validationResult?.isValid === true
    );

    if (validAccounts.length === 0) {
      this.setState({ isValid: false });
      return;
    }

    // Check if the total matches the invoice total
    const total = validAccounts.reduce(
      (sum, account) => sum + account.amount,
      0
    );
    const isValid = Math.abs(total - invoice.total) < 0.01; // Allow for small floating point differences

    this.setState({ isValid });
  };

  private handleSubmit = async () => {
    const { invoice } = this.props;
    const { rechargeAccounts, isValid } = this.state;

    if (!isValid) {
      this.setState({
        errorMessage:
          'Please provide at least one valid debit chart string and ensure the total matches the invoice total.'
      });
      return;
    }

    this.setState({ isSaving: true, errorMessage: '' });

    try {
      const response = await fetch(`/recharge/pay/${invoice.linkId}`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          RequestVerificationToken: antiForgeryToken
        },
        body: JSON.stringify({
          debitRechargeAccounts: rechargeAccounts.filter(
            account => account.direction === 'Debit'
          )
        })
      });

      if (response.ok) {
        // Success - redirect or show success message
        window.location.href = `/recharge/pay/${invoice.linkId}?success=true`;
      } else {
        const errorData = await response.json();
        this.setState({
          errorMessage:
            errorData.message ||
            'An error occurred while submitting the invoice. Please try again.',
          isSaving: false
        });
      }
    } catch (error) {
      this.setState({
        errorMessage:
          'An error occurred while submitting the invoice. Please try again.',
        isSaving: false
      });
    }
  };

  private getFileIcon = (contentType: string): string => {
    if (contentType === 'application/pdf') {
      return 'far fa-file-pdf';
    }

    if (contentType.startsWith('image')) {
      return 'far fa-file-image';
    }

    return 'far fa-file';
  };

  private getSizeText = (size: number): string => {
    if (size <= 0) {
      return '';
    }

    if (size <= 1024) {
      return `${size.toFixed(0)} B`;
    }

    if (size <= 1024 * 1024) {
      return `${(size / 1024).toFixed(0)} KB`;
    }

    return `${(size / 1024 / 1024).toFixed(1)} MB`;
  };
}
