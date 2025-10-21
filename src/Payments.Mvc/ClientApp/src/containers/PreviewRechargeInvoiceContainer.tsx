import * as React from 'react';
import { RechargeInvoiceModel } from './PayInvoiceContainer';

interface IProps {
  invoice: RechargeInvoiceModel;
}

export default class PreviewRechargeInvoiceContainer extends React.Component<
  IProps,
  {}
> {
  public render() {
    const { invoice } = this.props;

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

              <div style={{ alignContent: 'center' }}>
                <button
                  type='button'
                  className='btn-gold btn-lg pay-now-button'
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

            {/* Display existing recharge accounts in preview mode */}
            {invoice.debitRechargeAccounts &&
              invoice.debitRechargeAccounts.length > 0 && (
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
                      {invoice.debitRechargeAccounts.map((account, index) => (
                        <tr key={account.id || index}>
                          <td>{account.financialSegmentString || 'N/A'}</td>
                          <td>
                            $
                            {account.amount
                              ? account.amount.toFixed(2)
                              : '0.00'}
                          </td>
                          <td>{account.notes || ''}</td>
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
              <a
                href={`/pdf/${invoice.linkId}`}
                className='btn-inverse'
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
