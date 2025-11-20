import * as React from 'react';
import { RechargeInvoiceModel } from './PayInvoiceContainer';
import RechargeAccountsControl from '../components/rechargeAccountsControl';
import { InvoiceRechargeItem } from '../models/InvoiceRechargeItem';

interface IProps {
  invoice: RechargeInvoiceModel;
}

interface IState {
  rechargeAccounts: InvoiceRechargeItem[];
  isValid: boolean;
  isValidating: boolean;
}

export default class PreviewRechargeInvoiceContainer extends React.Component<
  IProps,
  IState
> {
  private _rechargeAccountsRef: RechargeAccountsControl;

  constructor(props: IProps) {
    super(props);

    this.state = {
      rechargeAccounts: props.invoice.debitRechargeAccounts || [],
      isValid: false,
      isValidating: true
    };
  }

  componentDidMount() {
    // Validation will be triggered by the child component's onValidationComplete callback
  }
  public render() {
    const { invoice } = this.props;
    const { rechargeAccounts, isValid, isValidating } = this.state;

    console.log('PreviewRechargeInvoiceContainer rendering', invoice);

    return (
      <>
        {/* Preview Warning Banner */}
        <div className='card preview-warning-card'>
          <span>
            <i className='fas fa-exclamation-triangle preview-warning-icon me-2'></i>
            This is a preview page only. You cannot submit this invoice here.
          </span>
        </div>

        {/* Invoice Card */}
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

              {isValidating && (
                <div className='alert alert-info mt-3' role='alert'>
                  <i className='fas fa-spinner fa-spin me-2'></i>
                  Validating form...
                </div>
              )}

              {!isValidating && !isValid && (
                <div className='alert alert-warning mt-3' role='alert'>
                  <i className='fas fa-exclamation-triangle me-2'></i>
                  Form validation status (preview only):
                  <ul className='mb-0 mt-2'>
                    <li>At least one valid debit chart string is required</li>
                    <li>All chart strings must be validated successfully</li>
                    <li>
                      Total debit amounts must equal the invoice total ($
                      {invoice.total.toFixed(2)})
                    </li>
                    <li>All amounts must be greater than zero</li>
                  </ul>
                </div>
              )}

              {!isValidating && isValid && (
                <div className='alert alert-success mt-3' role='alert'>
                  <i className='fas fa-check-circle me-2'></i>
                  Form is valid (preview only)
                </div>
              )}

              <div style={{ alignContent: 'center' }}>
                <button
                  type='button'
                  className='btn btn-secondary'
                  disabled={true}
                  aria-disabled='true'
                  title='Preview mode - submission is disabled'
                  style={{ opacity: 0.6, cursor: 'not-allowed' }}
                >
                  <i className='fas fa-check me-3' aria-hidden='true' />
                  Submit for Approval
                </button>
              </div>
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

            {/* Recharge Accounts - Editable in preview mode for demonstration */}
            <div className='card-body invoice-recharge-accounts'>
              <h3>Debit Chart Strings (Preview)</h3>
              <p className='text-muted'>
                You can interact with the chart strings below to see how the
                form works. Changes cannot be saved in preview mode.
              </p>
              <RechargeAccountsControl
                ref={r => (this._rechargeAccountsRef = r)}
                rechargeAccounts={rechargeAccounts}
                invoiceTotal={invoice.total}
                onChange={this.handleRechargeAccountsChange}
                showCreditAccounts={false}
                onValidationComplete={this.handleValidationComplete}
              />
            </div>

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
              <a
                href={`/pdf/${invoice.linkId}`}
                className='btn btn-outline-primary'
                download
              >
                Download PDF
              </a>
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
      </>
    );
  }

  private handleRechargeAccountsChange = (accounts: InvoiceRechargeItem[]) => {
    this.setState({ rechargeAccounts: accounts }, () => {
      this.validateForm();
    });
  };

  private handleValidationComplete = () => {
    console.log('Child component validation complete, validating form');
    // The child component has already ensured all state updates are complete before calling this
    // We can safely validate the form immediately
    this.validateForm();
    this.setState({ isValidating: false });
  };

  private validateForm = () => {
    const { rechargeAccounts } = this.state;
    const { invoice } = this.props;

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

    // Get all debit accounts with chart strings (including those with invalid amounts)
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

    // Check if any debit accounts have invalid amounts (zero or negative)
    const hasInvalidAmounts = allDebitAccounts.some(
      account => account.amount <= 0
    );

    if (hasInvalidAmounts) {
      console.log(
        'Form validation failed: one or more debit accounts have invalid amounts (must be greater than zero)'
      );
      this.setState({ isValid: false });
      return;
    }

    // Check if the total matches the invoice total
    const total = allDebitAccounts.reduce(
      (sum, account) => sum + account.amount,
      0
    );
    const totalMatches = Math.abs(total - invoice.total) < 0.01; // Allow for small floating point differences

    if (!totalMatches) {
      console.log(
        `Form validation failed: total mismatch (debit total: ${total}, invoice total: ${invoice.total})`
      );
      this.setState({ isValid: false });
      return;
    }

    console.log(
      `Form validation passed: all validations successful (total: ${total}, invoice total: ${invoice.total})`
    );
    this.setState({ isValid: true });
  };

  private getFileIcon = (contentType: string) => {
    if (contentType === 'application/pdf') {
      return 'far fa-file-pdf';
    }

    if (contentType.startsWith('image')) {
      return 'far fa-file-image';
    }

    return 'far fa-file';
  };

  private getSizeText = (size: number) => {
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
