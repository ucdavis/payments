import * as React from 'react';

import {
  AccountValidationModel,
  InvoiceRechargeItem
} from '../models/InvoiceRechargeItem';

interface IProps {
  rechargeAccount?: InvoiceRechargeItem;
  invoiceTotal: number;
  canEdit: boolean;
  onChange: (rechargeAccount?: InvoiceRechargeItem) => void;
}

interface IState {
  validationResult?: AccountValidationModel;
  isValidating: boolean;
  hasValidationError: boolean;
}

export default class CreditCardAccountOverrideControl extends React.Component<
  IProps,
  IState
> {
  private inputRef: HTMLInputElement | null = null;
  private validationAbortController?: AbortController;
  private isComponentMounted = false;

  constructor(props: IProps) {
    super(props);

    this.state = {
      validationResult: undefined,
      isValidating: false,
      hasValidationError: false
    };
  }

  async componentDidMount() {
    this.isComponentMounted = true;

    const chartString = this.props.rechargeAccount?.financialSegmentString;
    if (this.props.canEdit && chartString && chartString.trim()) {
      await this.validateChartString(chartString);
    }
  }

  componentWillUnmount() {
    this.isComponentMounted = false;
    this.validationAbortController?.abort();
    this.validationAbortController = undefined;
  }

  componentDidUpdate(prevProps: IProps) {
    const chartString = this.props.rechargeAccount?.financialSegmentString;
    if (
      chartString &&
      prevProps.invoiceTotal !== this.props.invoiceTotal &&
      this.props.rechargeAccount?.amount !== this.props.invoiceTotal
    ) {
      this.props.onChange(this.createRechargeAccount(chartString));
    }
  }

  public hasValidationErrors = (): boolean => {
    return (
      this.state.isValidating ||
      this.state.hasValidationError ||
      (this.state.validationResult
        ? !this.state.validationResult.isValid
        : false)
    );
  };

  public render() {
    const { canEdit, rechargeAccount } = this.props;
    const { isValidating } = this.state;
    const chartString = rechargeAccount?.financialSegmentString || '';

    return (
      <div className='form-group credit-card-account-override-control'>
        <label>Income Account Override</label>
        <div className='input-group'>
          <input
            ref={el => {
              this.inputRef = el;
            }}
            type='text'
            className={`form-control ${
              isValidating
                ? 'validation-input'
                : 'validation-input input-normal'
            }`}
            placeholder='Financial Segment String'
            value={chartString}
            onChange={this.handleChartStringChange}
            onBlur={this.handleChartStringBlur}
            maxLength={70}
            disabled={!canEdit || isValidating}
          />
          {isValidating && (
            <div className='input-group-text validation-spinner-container'>
              <div
                className='spinner-border spinner-border-sm validation-spinner'
                role='status'
              >
                <span className='visually-hidden'>Validating...</span>
              </div>
            </div>
          )}
          {canEdit && (
            <div className='input-group-text'>
              <button
                type='button'
                className='btn btn-primary btn-icon'
                title='CCOA Picker'
                onClick={this.handleFinancialSegmentPicker}
                disabled={isValidating}
              >
                <i className='fas fa-search'></i>
              </button>
            </div>
          )}
          {chartString && (
            <div className='input-group-text'>
              <a
                href={`https://finjector.ucdavis.edu/details/${chartString}`}
                target='_blank'
                rel='noopener noreferrer'
                className={`btn btn-primary btn-icon ${
                  isValidating ? 'disabled' : ''
                }`}
                title='View In Finjector'
                aria-disabled={isValidating}
                onClick={e => {
                  if (isValidating) {
                    e.preventDefault();
                  }
                }}
              >
                <i className='fas fa-external-link-alt'></i>
              </a>
            </div>
          )}
        </div>
        {this.renderValidationMessages()}
      </div>
    );
  }

  private handleChartStringChange = (
    e: React.ChangeEvent<HTMLInputElement>
  ) => {
    const chartString = e.target.value;
    this.props.onChange(this.createRechargeAccount(chartString));

    this.setState({
      validationResult: undefined,
      hasValidationError: false
    });

    this.setChartStringCustomValidity(chartString.trim() ? false : true);
  };

  private handleChartStringBlur = () => {
    const chartString =
      this.props.rechargeAccount?.financialSegmentString || '';
    if (!chartString.trim()) {
      this.setChartStringCustomValidity(true);
      return;
    }

    this.validateChartString(chartString);
  };

  private handleFinancialSegmentPicker = async () => {
    try {
      if (typeof window !== 'undefined' && (window as any).Finjector) {
        const chart = await (window as any).Finjector.findChartSegmentString();
        if (chart && chart.status === 'success') {
          const rechargeAccount = this.createRechargeAccount(chart.data);
          this.props.onChange(rechargeAccount);
          await this.validateChartString(chart.data);
        } else {
          alert('Something went wrong with the CCOA picker');
        }
      } else {
        alert(
          'CCOA picker service is not available. Please ensure the page has loaded completely.'
        );
      }
    } catch (error) {
      console.error('Error with CCOA picker:', error);
      alert('An error occurred while using the CCOA picker');
    }
  };

  private validateChartString = async (chartString: string) => {
    if (!this.isComponentMounted) {
      return;
    }

    this.validationAbortController?.abort();
    const abortController = new AbortController();
    this.validationAbortController = abortController;

    this.setState({
      isValidating: true,
      hasValidationError: false
    });
    this.setChartStringCustomValidity(false);

    try {
      const response = await fetch(
        `/api/financial-accounts/validate?chartString=${encodeURIComponent(
          chartString
        )}`,
        {
          method: 'GET',
          headers: {
            'Content-Type': 'application/json'
          },
          signal: abortController.signal
        }
      );

      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }

      const validationResult: AccountValidationModel = await response.json();
      const normalizedChartString = validationResult.chartString || chartString;

      if (!this.isComponentMounted || abortController.signal.aborted) {
        return;
      }

      if (normalizedChartString !== chartString) {
        this.props.onChange(this.createRechargeAccount(normalizedChartString));
      }

      this.setState({
        validationResult,
        isValidating: false,
        hasValidationError: !validationResult.isValid
      });
      this.setChartStringCustomValidity(validationResult.isValid);
    } catch (error) {
      if (!this.isComponentMounted || abortController.signal.aborted) {
        return;
      }

      console.error('Error validating chart string:', error);
      this.setState({
        validationResult: {
          isValid: false,
          chartString,
          messages: ['Failed to validate chart string. Please try again.'],
          warnings: [],
          details: []
        },
        isValidating: false,
        hasValidationError: true
      });
      this.setChartStringCustomValidity(false);
    } finally {
      if (this.validationAbortController === abortController) {
        this.validationAbortController = undefined;
      }
    }
  };

  private createRechargeAccount = (
    financialSegmentString: string
  ): InvoiceRechargeItem | undefined => {
    const trimmed = financialSegmentString.trim();
    if (!trimmed) {
      return undefined;
    }

    return {
      id: this.props.rechargeAccount?.id || 0,
      direction: 'Credit',
      financialSegmentString: trimmed,
      amount: this.props.invoiceTotal,
      percentage: 100,
      enteredByKerb: this.props.rechargeAccount?.enteredByKerb,
      enteredByName: this.props.rechargeAccount?.enteredByName,
      notes: this.props.rechargeAccount?.notes || ''
    };
  };

  private setChartStringCustomValidity = (isValid: boolean) => {
    if (!this.inputRef) {
      return;
    }

    this.inputRef.setCustomValidity(isValid ? '' : 'This has errors');
  };

  private renderValidationMessages = () => {
    const { validationResult } = this.state;
    const hasChartString =
      this.props.rechargeAccount?.financialSegmentString &&
      this.props.rechargeAccount.financialSegmentString.trim().length > 0;

    if (!validationResult) {
      return null;
    }

    const hasMessages = validationResult.messages.length > 0;
    const hasWarnings = validationResult.warnings.length > 0;

    return (
      <div className='validation-messages mt-1'>
        {hasMessages && (
          <div className='alert alert-danger alert-sm mb-1 py-1 px-2'>
            <small>
              <strong>Errors:</strong>
              <ul className='mb-0 mt-1 ps-3'>
                {validationResult.messages.map((message, index) => (
                  <li key={index}>{message}</li>
                ))}
              </ul>
            </small>
          </div>
        )}
        {hasWarnings && (
          <div className='alert alert-warning alert-sm mb-1 py-1 px-2'>
            <small>
              <strong>Warnings:</strong>
              <ul className='mb-0 mt-1 ps-3'>
                {validationResult.warnings.map((warning, index) => (
                  <li key={index}>
                    <strong>{warning.key}:</strong> {warning.value}
                  </li>
                ))}
              </ul>
            </small>
          </div>
        )}
        {validationResult.isValid && !hasMessages && hasChartString && (
          <div className='alert alert-success alert-sm mb-1 py-1 px-2'>
            <small>
              <i className='fas fa-check-circle me-1'></i>
              <strong>Valid:</strong> Chart string validation passed
              successfully
            </small>
          </div>
        )}
      </div>
    );
  };
}
