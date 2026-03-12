import * as React from 'react';
import { formatCurrencyLocale } from '../utils/currency';

interface IProps {
  dueDate: string | null;
  errorMessage: string;
  isSaving: boolean;
  isValid: boolean;
  isValidating: boolean;
  onSubmit: () => void;
  total: number;
}

export default class PayInvoiceAction extends React.Component<IProps> {
  public render() {
    const {
      dueDate,
      errorMessage,
      isSaving,
      isValid,
      isValidating,
      onSubmit,
      total
    } = this.props;

    return (
      <div className='pay-action'>
        <span className='pay-action-total'>{formatCurrencyLocale(total)}</span>

        {dueDate && (
          <span className='pay-action-date secondary-font'>
            Due{' '}
            {new Date(dueDate).toLocaleDateString('en-US', {
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
            Please complete all required fields before submitting:
            <ul className='mb-0 mt-2'>
              <li>At least one valid debit chart string is required</li>
              <li>All chart strings must be validated successfully</li>
              <li>
                Total debit amounts must equal the invoice total (
                {formatCurrencyLocale(total)})
              </li>
              <li>All amounts must be greater than zero</li>
            </ul>
          </div>
        )}

        {!isValidating && isValid && !isSaving && (
          <div className='alert alert-success mt-3' role='alert'>
            <i className='fas fa-check-circle me-2'></i>
            Form is valid and ready to submit
          </div>
        )}

        <div style={{ alignContent: 'center' }}>
          <button
            type='button'
            className='btn btn-primary btn-lg'
            onClick={onSubmit}
            disabled={!isValid || isSaving || isValidating}
            title={
              isValidating
                ? 'Validating form...'
                : !isValid
                ? 'Please complete all required fields'
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
                Submit for Approval
              </>
            )}
          </button>
        </div>
      </div>
    );
  }
}
