import * as React from 'react';
import RechargeAccountsControl from '../components/rechargeAccountsControl';
import { InvoiceRechargeItem } from '../models/InvoiceRechargeItem';
import { formatCurrencyLocale } from '../utils/currency';

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
  debitRechargeAccounts: InvoiceRechargeItem[]; // Accounts that can be edited/approved
  displayDebitRechargeAccounts: InvoiceRechargeItem[]; // Read-only accounts
  canApprove: boolean;
  message?: string;
  errorMessage?: string;
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
  message: string;
  isValidating: boolean;
  showRejectDialog: boolean;
  rejectReason: string;
}

export default class FinancialApproveInvoiceContainer extends React.Component<
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
      errorMessage: '',
      message: '',
      isValidating: true,
      showRejectDialog: false,
      rejectReason: ''
    };
  }

  componentDidMount() {
    // Validation will be triggered by the child component's onValidationComplete callback
  }

  private handleValidationComplete = () => {
    console.log('Child component validation complete, validating form');
    // The child component has already ensured all state updates are complete before calling this
    // We can safely validate the form immediately
    this.validateForm();
    this.setState({ isValidating: false });
  };

  public render() {
    const { invoice } = this.props;
    const {
      rechargeAccounts,
      isValid,
      isSaving,
      errorMessage,
      isValidating,
      showRejectDialog,
      rejectReason
    } = this.state;
    const canEdit = invoice.status === 'PendingApproval' && invoice.canApprove;

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

          {/* Messages from server */}
          {invoice.errorMessage && (
            <div className='alert alert-danger mx-3' role='alert'>
              <i className='fas fa-exclamation-circle me-2'></i>
              {invoice.errorMessage}
            </div>
          )}

          {invoice.message && !invoice.errorMessage && (
            <div className='alert alert-info mx-3' role='alert'>
              <i className='fas fa-info-circle me-2'></i>
              {invoice.message}
            </div>
          )}

          {/* Payment Action Area */}
          <div className='pay-action'>
            {canEdit && (
              <>
                <span className='pay-action-total'>
                  {formatCurrencyLocale(invoice.total)}
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

                {isValidating && (
                  <div className='alert alert-info mt-3' role='alert'>
                    <i className='fas fa-spinner fa-spin me-2'></i>
                    Validating form...
                  </div>
                )}

                {!isValidating && !isValid && !isSaving && (
                  <div className='alert alert-warning mt-3' role='alert'>
                    <i className='fas fa-exclamation-triangle me-2'></i>
                    Please review the following to approve:
                    <ul className='mb-0 mt-2'>
                      <li>All chart strings must be validated successfully</li>
                    </ul>
                  </div>
                )}

                {!isValidating && isValid && !isSaving && (
                  <div className='alert alert-success mt-3' role='alert'>
                    <i className='fas fa-check-circle me-2'></i>
                    Chart strings are valid and ready to approve
                  </div>
                )}

                <div
                  style={{
                    display: 'flex',
                    gap: '10px',
                    justifyContent: 'center'
                  }}
                >
                  <button
                    type='button'
                    className='btn btn-primary btn-lg'
                    onClick={this.handleApprove}
                    disabled={!isValid || isSaving || isValidating}
                    title={
                      isValidating
                        ? 'Validating form...'
                        : !isValid
                        ? 'Please ensure all chart strings are valid'
                        : ''
                    }
                  >
                    {isSaving ? (
                      <>
                        <i className='fas fa-spinner fa-spin me-3' />
                        Processing...
                      </>
                    ) : (
                      <>
                        <i className='fas fa-check' aria-hidden='true' />
                        Approve
                      </>
                    )}
                  </button>

                  <button
                    type='button'
                    className='btn btn-danger btn-lg'
                    onClick={this.handleRejectClick}
                    disabled={isSaving || isValidating}
                    title={isValidating ? 'Validating form...' : ''}
                  >
                    {isSaving ? (
                      <>
                        <i className='fas fa-spinner fa-spin me-3' />
                        Processing...
                      </>
                    ) : (
                      <>
                        <i className='fas fa-times' aria-hidden='true' />
                        Reject
                      </>
                    )}
                  </button>
                </div>
              </>
            )}

            {!canEdit && (
              <>
                <span className='pay-action-total'>
                  {formatCurrencyLocale(invoice.total)}
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

                <div className='alert alert-secondary mt-3' role='alert'>
                  <i className='fas fa-info-circle me-2'></i>
                  Status: {invoice.status}. This invoice cannot be acted upon.
                </div>
              </>
            )}

            {invoice.paid && (
              <>
                <h1>Invoice Paid</h1>
                {invoice.paidDate && (
                  <h2>
                    {formatCurrencyLocale(invoice.total)} USD paid{' '}
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
                    <td>{formatCurrencyLocale(item.amount)}</td>
                    <td>{formatCurrencyLocale(item.total)}</td>
                  </tr>
                ))}
                <tr>
                  <td></td>
                  <td>Total:</td>
                  <td></td>
                  <td>{formatCurrencyLocale(invoice.total)}</td>
                </tr>
              </tbody>
            </table>
          </div>

          {/* Editable Recharge Accounts - Only editable if canApprove */}
          {canEdit && rechargeAccounts.length > 0 && (
            <div className='card-body invoice-recharge-accounts'>
              <h3>Debit Chart Strings to Approve</h3>
              <p className='text-muted'>
                Review and edit chart strings as needed. Only the chart string
                can be modified.
              </p>
              <RechargeAccountsControl
                ref={r => {
                  this._rechargeAccountsRef = r;
                }}
                rechargeAccounts={rechargeAccounts}
                invoiceTotal={invoice.total}
                onChange={this.handleRechargeAccountsChange}
                showCreditAccounts={false}
                onValidationComplete={this.handleValidationComplete}
                fromApprove={true}
              />
            </div>
          )}

          {/* Display-only Debit Recharge Accounts */}
          {invoice.displayDebitRechargeAccounts &&
            invoice.displayDebitRechargeAccounts.length > 0 && (
              <div className='card-body'>
                <h3>Debit Chart Strings</h3>
                <p className='text-muted'>
                  These chart strings have already been approved or are assigned
                  to other approvers.
                </p>
                <table className='table'>
                  <thead>
                    <tr>
                      <th>Chart String</th>
                      <th>Amount</th>
                      <th>Percentage</th>
                      <th>Status</th>
                      <th>Notes</th>
                    </tr>
                  </thead>
                  <tbody>
                    {invoice.displayDebitRechargeAccounts.map(
                      (account: any) => (
                        <tr key={account.id}>
                          <td>
                            <a
                              href={`https://finjector.ucdavis.edu/details/${account.financialSegmentString}`}
                              target='_blank'
                              rel='noopener noreferrer'
                            >
                              {account.financialSegmentString}
                            </a>
                          </td>
                          <td>
                            {account.amount.toLocaleString('en-US', {
                              style: 'currency',
                              currency: 'USD'
                            })}
                          </td>
                          <td>
                            {account.percentage > 0
                              ? `${account.percentage.toFixed(2)}%`
                              : ''}
                          </td>
                          <td>
                            {account.approvedByName ? (
                              <span className='text-success'>
                                <i className='fas fa-check-circle me-1'></i>
                                Approved by {account.approvedByName}
                              </span>
                            ) : (
                              <span className='text-warning'>
                                <i className='fas fa-clock me-1'></i>
                                Pending Approval
                              </span>
                            )}
                          </td>
                          <td>{account.notes}</td>
                        </tr>
                      )
                    )}
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
            <br />
            <br />
            {invoice.paid ? (
              <a
                href={`/receipt/${invoice.linkId}`}
                className='btn btn-outline-primary'
                download
              >
                Download PDF
              </a>
            ) : (
              <a
                href={`/pdf/${invoice.linkId}`}
                className='btn btn-outline-primary'
                download
              >
                Download PDF
              </a>
            )}
          </div>

          <div className='pay-footer'>
            <span>
              If you have any questions, contact us
              {invoice.team.contactEmail && (
                <>
                  {' at '}
                  <a href={`mailto:${invoice.team.contactEmail}`}>
                    {invoice.team.contactEmail}
                  </a>
                </>
              )}
              {invoice.team.contactEmail &&
                invoice.team.contactPhoneNumber &&
                ' or'}
              {invoice.team.contactPhoneNumber && (
                <> call at {invoice.team.contactPhoneNumber}</>
              )}
              {!invoice.team.contactEmail &&
                !invoice.team.contactPhoneNumber &&
                '.'}
            </span>
          </div>
        </div>

        {/* Reject Dialog */}
        {showRejectDialog && (
          <div
            className='modal'
            style={{ display: 'block', backgroundColor: 'rgba(0,0,0,0.5)' }}
          >
            <div className='modal-dialog'>
              <div className='modal-content'>
                <div className='modal-header'>
                  <h5 className='modal-title'>Reject Invoice</h5>
                  <button
                    type='button'
                    className='btn-close'
                    onClick={this.handleCancelReject}
                    aria-label='Close'
                  ></button>
                </div>
                <div className='modal-body'>
                  <p>Please provide a reason for rejecting this invoice:</p>
                  <textarea
                    className='form-control'
                    rows={4}
                    value={rejectReason}
                    onChange={e =>
                      this.setState({ rejectReason: e.target.value })
                    }
                    placeholder='Enter rejection reason...'
                    autoFocus
                  />
                </div>
                <div className='modal-footer'>
                  <button
                    type='button'
                    className='btn btn-outline-danger'
                    onClick={this.handleCancelReject}
                  >
                    Cancel
                  </button>
                  <button
                    type='button'
                    className='btn btn-danger'
                    onClick={this.handleConfirmReject}
                    disabled={!rejectReason.trim()}
                  >
                    Confirm Rejection
                  </button>
                </div>
              </div>
            </div>
          </div>
        )}
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

    // Check if any validations are in progress
    if (this._rechargeAccountsRef && this._rechargeAccountsRef.isValidating()) {
      console.log('Form validation pending: validation in progress');
      this.setState({ isValid: false });
      return;
    }

    // Check if component has validation errors
    if (
      this._rechargeAccountsRef &&
      this._rechargeAccountsRef.hasValidationErrors()
    ) {
      console.log(
        'Form validation failed: child component has validation errors'
      );
      this.setState({ isValid: false });
      return;
    }

    // For approval, we just need valid chart strings
    // No need to validate totals or amounts since they're read-only
    const allDebitAccounts = rechargeAccounts.filter(
      account =>
        account.direction === 'Debit' &&
        account.financialSegmentString &&
        account.financialSegmentString.trim() !== ''
    );

    if (allDebitAccounts.length === 0) {
      console.log('Form validation failed: no debit accounts');
      this.setState({ isValid: false });
      return;
    }

    console.log('Form validation passed: all chart strings are valid');
    this.setState({ isValid: true });
  };

  private handleRejectClick = () => {
    this.setState({ showRejectDialog: true, rejectReason: '' });
  };

  private handleCancelReject = () => {
    this.setState({ showRejectDialog: false, rejectReason: '' });
  };

  private handleConfirmReject = async () => {
    const { invoice } = this.props;
    const { rejectReason } = this.state;

    if (!rejectReason.trim()) {
      this.setState({ errorMessage: 'Please provide a reason for rejection.' });
      return;
    }

    this.setState({
      isSaving: true,
      errorMessage: '',
      showRejectDialog: false
    });

    try {
      const response = await fetch(
        `/recharge/financialapprove/${
          invoice.linkId
        }?actionType=Reject&rejectReason=${encodeURIComponent(rejectReason)}`,
        {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
            RequestVerificationToken: antiForgeryToken
          },
          body: JSON.stringify([])
        }
      );

      if (response.ok) {
        // Success - redirect to reload the page with updated invoice data
        window.location.href = `/recharge/financialapprove/${invoice.linkId}`;
      } else {
        const errorData = await response.json();
        if (errorData.errorMessage) {
          this.setState({
            errorMessage: errorData.errorMessage,
            isSaving: false
          });
        } else if (errorData.message) {
          this.setState({
            errorMessage: errorData.message,
            isSaving: false
          });
        } else {
          this.setState({
            errorMessage:
              'An error occurred while rejecting the invoice. Please try again.',
            isSaving: false
          });
        }
      }
    } catch (error) {
      this.setState({
        errorMessage:
          'An error occurred while rejecting the invoice. Please try again.',
        isSaving: false
      });
    }
  };

  private handleApprove = async () => {
    console.log('Approving invoice...');
    const { invoice } = this.props;
    const { rechargeAccounts, isValid } = this.state;

    if (!isValid) {
      console.log('Form is invalid');
      this.setState({
        errorMessage:
          'Please ensure all chart strings are valid before approving.'
      });
      return;
    }

    console.log('Form is valid');
    this.setState({ isSaving: true, errorMessage: '' });

    try {
      // Filter to only debit accounts and map to the format expected by the backend
      const debitAccounts = rechargeAccounts
        .filter(account => account.direction === 'Debit')
        .map(account => ({
          ...account,
          direction: 1 // Debit = 1
        }));

      const response = await fetch(
        `/recharge/financialapprove/${invoice.linkId}?actionType=Approve`,
        {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
            RequestVerificationToken: antiForgeryToken
          },
          body: JSON.stringify(debitAccounts)
        }
      );

      if (response.ok) {
        // Success - redirect to reload the page with updated invoice data
        window.location.href = `/recharge/financialapprove/${invoice.linkId}`;
      } else {
        const errorData = await response.json();
        if (errorData.errorMessage) {
          this.setState({
            errorMessage: errorData.errorMessage,
            isSaving: false
          });
        } else if (errorData.message) {
          this.setState({
            errorMessage: errorData.message,
            isSaving: false
          });
        } else {
          this.setState({
            errorMessage:
              'An error occurred while approving the invoice. Please try again.',
            isSaving: false
          });
        }
      }
    } catch (error) {
      this.setState({
        errorMessage:
          'An error occurred while approving the invoice. Please try again.',
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
