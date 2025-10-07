import * as React from 'react';

import {
  InvoiceRechargeItem,
  AccountValidationModel
} from '../models/InvoiceRechargeItem';

import CurrencyControl from './currencyControl';
import NumberControl from './numberControl';

interface IProps {
  rechargeAccounts: InvoiceRechargeItem[];
  invoiceTotal: number;
  onChange: (rechargeAccounts: InvoiceRechargeItem[]) => void;
}

interface IState {
  creditAccounts: InvoiceRechargeItem[];
  debitAccounts: InvoiceRechargeItem[];
  nextId: number;
  isInternalUpdate: boolean; // Flag to track internal updates vs external prop changes
}

export default class RechargeAccountsControl extends React.Component<
  IProps,
  IState
> {
  // Helper function to normalize direction values from server
  private normalizeDirection = (direction: any): 'Credit' | 'Debit' => {
    // Handle integer enum values: 0 = Credit, 1 = Debit
    if (direction === 0 || direction === '0') return 'Credit';
    if (direction === 1 || direction === '1') return 'Debit';
    // Handle string values
    if (typeof direction === 'string') {
      return direction as 'Credit' | 'Debit';
    }
    // Default fallback
    return 'Credit';
  };

  constructor(props: IProps) {
    super(props);

    // Normalize direction values and separate credit and debit accounts
    const normalizedAccounts = props.rechargeAccounts.map(account => ({
      ...account,
      direction: this.normalizeDirection(account.direction),
      validationResult: undefined,
      isValidating: false,
      hasValidationError: false,
      skipNextValidation: false
    }));

    const creditAccounts = normalizedAccounts.filter(
      account => account.direction === 'Credit'
    );
    const debitAccounts = normalizedAccounts.filter(
      account => account.direction === 'Debit'
    );

    // Ensure we have at least one credit account
    if (creditAccounts.length === 0) {
      const newAccount = this.createNewAccount('Credit', 1);
      creditAccounts.push({
        ...newAccount,
        validationResult: undefined,
        isValidating: false,
        hasValidationError: false,
        skipNextValidation: false
      });
    }

    const nextId = Math.max(...props.rechargeAccounts.map(a => a.id), 0) + 1;

    this.state = {
      creditAccounts,
      debitAccounts,
      nextId,
      isInternalUpdate: false
    };

    // Validate existing financial segment strings after component mounts
    // Only validate if there are accounts with existing financial segment strings
    const hasExistingData = props.rechargeAccounts.some(
      account =>
        account.financialSegmentString && account.financialSegmentString.trim()
    );

    if (hasExistingData) {
      setTimeout(() => {
        this.validateExistingAccounts();
      }, 100);
    }
  }

  componentDidUpdate(prevProps: IProps) {
    // If the rechargeAccounts prop changes, update our state
    if (prevProps.rechargeAccounts !== this.props.rechargeAccounts) {
      // If this was an internal update (we added/removed accounts), don't re-validate
      if (this.state.isInternalUpdate) {
        console.log(
          'Internal update detected, skipping re-validation and resetting flag'
        );
        this.setState({ isInternalUpdate: false });
        return;
      }

      // Check if this is actually NEW data from outside (not from our own updates)
      // by comparing the account count and IDs, not just the financial segment strings
      const prevAccountIds = prevProps.rechargeAccounts
        .map(a => `${a.id}-${a.direction}`)
        .sort()
        .join('|');
      const currentAccountIds = this.props.rechargeAccounts
        .map(a => `${a.id}-${a.direction}`)
        .sort()
        .join('|');

      // Only proceed if the accounts themselves changed (new/removed accounts), not just field updates
      if (prevAccountIds !== currentAccountIds) {
        console.log(
          'Account structure changed, reinitializing component state'
        );

        const normalizedAccounts = this.props.rechargeAccounts.map(account => ({
          ...account,
          direction: this.normalizeDirection(account.direction),
          validationResult: undefined,
          isValidating: false,
          hasValidationError: false,
          skipNextValidation: false
        }));

        const creditAccounts = normalizedAccounts.filter(
          account => account.direction === 'Credit'
        );
        const debitAccounts = normalizedAccounts.filter(
          account => account.direction === 'Debit'
        );

        // Ensure we have at least one credit account
        if (creditAccounts.length === 0) {
          const newAccount = this.createNewAccount('Credit', this.state.nextId);
          creditAccounts.push({
            ...newAccount,
            validationResult: undefined,
            isValidating: false,
            hasValidationError: false,
            skipNextValidation: false
          });
        }

        this.setState(
          {
            creditAccounts,
            debitAccounts
          },
          () => {
            // Only validate if there are accounts with financial segment strings
            const hasDataToValidate = this.props.rechargeAccounts.some(
              account =>
                account.financialSegmentString &&
                account.financialSegmentString.trim()
            );

            if (hasDataToValidate) {
              console.log(
                'Validating existing accounts after account structure change'
              );
              setTimeout(() => {
                this.validateExistingAccounts();
              }, 100);
            }
          }
        );
      } else {
        console.log(
          'Account field updated, but structure unchanged - not re-validating'
        );
      }
    }
  }

  private createNewAccount = (
    direction: 'Credit' | 'Debit',
    id: number
  ): InvoiceRechargeItem => ({
    id: 0,
    direction,
    financialSegmentString: '',
    amount: 0,
    percentage: 0,
    notes: '',
    validationResult: undefined,
    isValidating: false,
    hasValidationError: false,
    skipNextValidation: false
  });

  private validateChartString = async (
    chartString: string,
    direction: 'Credit' | 'Debit'
  ): Promise<AccountValidationModel | null> => {
    if (!chartString.trim()) {
      return null;
    }

    try {
      const directionValue = direction === 'Credit' ? 0 : 1;
      const response = await fetch(
        `/api/recharge/validate?chartString=${encodeURIComponent(
          chartString
        )}&direction=${directionValue}`,
        {
          method: 'GET',
          headers: {
            'Content-Type': 'application/json'
          }
        }
      );

      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }

      const validationResult = await response.json();
      return validationResult;
    } catch (error) {
      console.error('Error validating chart string:', error);
      return {
        isValid: false,
        chartString: chartString,
        messages: ['Failed to validate chart string. Please try again.'],
        warnings: [],
        details: []
      };
    }
  };

  private handleChartStringValidation = async (
    index: number,
    direction: 'Credit' | 'Debit',
    chartString: string
  ) => {
    console.log(
      `Starting validation for ${direction} account ${index} with chartString: ${chartString}`
    );

    const updateAccountSilent =
      direction === 'Credit'
        ? this.updateCreditAccountSilent
        : this.updateDebitAccountSilent;

    const updateAccount =
      direction === 'Credit'
        ? this.updateCreditAccount
        : this.updateDebitAccount;

    // Set loading state (using silent updates to avoid triggering parent onChange)
    updateAccountSilent(index, 'isValidating', true);
    updateAccountSilent(index, 'hasValidationError', false);

    let valueChanged = false;

    try {
      const validationResult = await this.validateChartString(
        chartString,
        direction
      );

      if (validationResult) {
        // Update the chart string if validation returned a corrected value
        if (validationResult.chartString !== chartString) {
          console.log(
            `Validation corrected chart string from "${chartString}" to "${validationResult.chartString}"`
          );

          // Set flag to skip next validation to prevent infinite loop
          updateAccountSilent(index, 'skipNextValidation', true);

          // Use normal update for the corrected financial segment string (this notifies parent)
          updateAccount(
            index,
            'financialSegmentString',
            validationResult.chartString
          );
          valueChanged = true;
        }

        updateAccountSilent(index, 'validationResult', validationResult);
        updateAccountSilent(
          index,
          'hasValidationError',
          !validationResult.isValid
        );
      } else {
        updateAccountSilent(index, 'validationResult', undefined);
        updateAccountSilent(index, 'hasValidationError', false);
      }
    } catch (error) {
      console.error('Validation error:', error);
      updateAccountSilent(index, 'hasValidationError', true);
    } finally {
      updateAccountSilent(index, 'isValidating', false);
    }

    console.log(
      `Validation completed for ${direction} account ${index}, value changed: ${valueChanged}`
    );
  };

  private handleFinancialSegmentBlur = (
    index: number,
    direction: 'Credit' | 'Debit',
    account: InvoiceRechargeItem
  ) => {
    const updateAccountSilent =
      direction === 'Credit'
        ? this.updateCreditAccountSilent
        : this.updateDebitAccountSilent;

    // Check if we should skip validation (to prevent infinite loop)
    if (account.skipNextValidation) {
      console.log(
        `Skipping validation for ${direction} account ${index} due to skipNextValidation flag`
      );
      updateAccountSilent(index, 'skipNextValidation', false);
      return;
    }

    // Notify parent of final value when user finishes typing
    console.log(
      `User finished typing in ${direction} account ${index}, notifying parent`
    );
    this.updateAccounts();

    // Don't validate empty strings
    if (
      !account.financialSegmentString ||
      !account.financialSegmentString.trim()
    ) {
      return;
    }

    console.log(
      `Triggering validation for ${direction} account ${index} with value: ${account.financialSegmentString}`
    );

    // Proceed with normal validation
    this.handleChartStringValidation(
      index,
      direction,
      account.financialSegmentString
    );
  };

  private validateExistingAccounts = async () => {
    // Validate all existing credit accounts
    for (let i = 0; i < this.state.creditAccounts.length; i++) {
      const account = this.state.creditAccounts[i];
      if (account.financialSegmentString.trim()) {
        // Set skipNextValidation to prevent onBlur interference
        this.updateCreditAccountSilent(i, 'skipNextValidation', true);
        await this.handleChartStringValidation(
          i,
          'Credit',
          account.financialSegmentString
        );
      }
    }

    // Validate all existing debit accounts
    for (let i = 0; i < this.state.debitAccounts.length; i++) {
      const account = this.state.debitAccounts[i];
      if (account.financialSegmentString.trim()) {
        // Set skipNextValidation to prevent onBlur interference
        this.updateDebitAccountSilent(i, 'skipNextValidation', true);
        await this.handleChartStringValidation(
          i,
          'Debit',
          account.financialSegmentString
        );
      }
    }
  };

  private updateAccounts = () => {
    const allAccounts = [
      ...this.state.creditAccounts,
      ...this.state.debitAccounts
    ];
    this.props.onChange(allAccounts);
  };

  // Silent update methods that don't trigger parent onChange (for validation updates)
  private updateCreditAccountSilent = (
    index: number,
    field: keyof InvoiceRechargeItem,
    value: any
  ) => {
    const { creditAccounts } = this.state;
    const updatedAccounts = [...creditAccounts];
    updatedAccounts[index] = { ...updatedAccounts[index], [field]: value };

    this.setState({ creditAccounts: updatedAccounts });
  };

  private updateDebitAccountSilent = (
    index: number,
    field: keyof InvoiceRechargeItem,
    value: any
  ) => {
    const { debitAccounts } = this.state;
    const updatedAccounts = [...debitAccounts];
    updatedAccounts[index] = { ...updatedAccounts[index], [field]: value };

    this.setState({ debitAccounts: updatedAccounts });
  };

  private updateCreditAccount = (
    index: number,
    field: keyof InvoiceRechargeItem,
    value: any
  ) => {
    const { creditAccounts } = this.state;
    const updatedAccounts = [...creditAccounts];
    updatedAccounts[index] = { ...updatedAccounts[index], [field]: value };

    this.setState(
      {
        creditAccounts: updatedAccounts,
        isInternalUpdate: true
      },
      this.updateAccounts
    );
  };

  private updateDebitAccount = (
    index: number,
    field: keyof InvoiceRechargeItem,
    value: any
  ) => {
    const { debitAccounts } = this.state;
    const updatedAccounts = [...debitAccounts];
    updatedAccounts[index] = { ...updatedAccounts[index], [field]: value };

    this.setState(
      {
        debitAccounts: updatedAccounts,
        isInternalUpdate: true
      },
      this.updateAccounts
    );
  };

  private addCreditAccount = () => {
    const { creditAccounts, nextId } = this.state;
    const newAccount = this.createNewAccount('Credit', nextId);

    this.setState(
      {
        creditAccounts: [...creditAccounts, newAccount],
        nextId: nextId + 1,
        isInternalUpdate: true
      },
      this.updateAccounts
    );
  };

  private addDebitAccount = () => {
    const { debitAccounts, nextId } = this.state;
    const newAccount = this.createNewAccount('Debit', nextId);

    this.setState(
      {
        debitAccounts: [...debitAccounts, newAccount],
        nextId: nextId + 1,
        isInternalUpdate: true
      },
      this.updateAccounts
    );
  };

  private removeCreditAccount = (index: number) => {
    const { creditAccounts } = this.state;
    if (creditAccounts.length > 1) {
      // Must have at least one credit account
      const updatedAccounts = creditAccounts.filter((_, i) => i !== index);
      this.setState(
        {
          creditAccounts: updatedAccounts,
          isInternalUpdate: true
        },
        this.updateAccounts
      );
    }
  };

  private removeDebitAccount = (index: number) => {
    const { debitAccounts } = this.state;
    const updatedAccounts = debitAccounts.filter((_, i) => i !== index);
    this.setState(
      {
        debitAccounts: updatedAccounts,
        isInternalUpdate: true
      },
      this.updateAccounts
    );
  };

  private calculateTotal = (accounts: InvoiceRechargeItem[]): number => {
    return accounts.reduce((sum, account) => sum + account.amount, 0);
  };

  private renderValidationMessages = (account: InvoiceRechargeItem) => {
    if (!account.validationResult) {
      return null;
    }

    const { validationResult } = account;
    const hasMessages = validationResult.messages.length > 0;
    const hasWarnings = validationResult.warnings.length > 0;
    const isValid = validationResult.isValid;

    if (!hasMessages && !hasWarnings && !isValid) {
      return null;
    }

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
      </div>
    );
  };

  private handleFinancialSegmentPicker = async (
    index: number,
    direction: 'Credit' | 'Debit'
  ) => {
    try {
      // Check if window.Finjector is available
      if (typeof window !== 'undefined' && (window as any).Finjector) {
        const chart = await (window as any).Finjector.findChartSegmentString();
        if (chart && chart.status === 'success') {
          console.log(
            `Finjector selected chart string: ${chart.data} for ${direction} account ${index}`
          );

          const updateAccountSilent =
            direction === 'Credit'
              ? this.updateCreditAccountSilent
              : this.updateDebitAccountSilent;

          const updateAccount =
            direction === 'Credit'
              ? this.updateCreditAccount
              : this.updateDebitAccount;

          // Set skipNextValidation to prevent any onBlur interference
          updateAccountSilent(index, 'skipNextValidation', true);

          // Update the field value and notify parent
          updateAccount(index, 'financialSegmentString', chart.data);

          // Validate the chart string manually
          console.log(
            `Finjector triggering validation for ${direction} account ${index}`
          );
          await this.handleChartStringValidation(index, direction, chart.data);

          console.log(
            `Finjector validation completed for ${direction} account ${index}`
          );
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

  private renderAccountRow = (
    account: InvoiceRechargeItem,
    index: number,
    direction: 'Credit' | 'Debit',
    accounts: InvoiceRechargeItem[]
  ) => {
    const isCredit = direction === 'Credit';
    const updateAccount = isCredit
      ? this.updateCreditAccount
      : this.updateDebitAccount;
    const removeAccount = isCredit
      ? this.removeCreditAccount
      : this.removeDebitAccount;
    const canRemove = isCredit ? this.state.creditAccounts.length > 1 : true;
    const isLastAccount = index === accounts.length - 1;

    return (
      <React.Fragment key={account.id}>
        <tr>
          <td className='cell-financial-segment'>
            <div className='input-group'>
              <input
                type='text'
                className={`form-control ${
                  account.isValidating
                    ? 'validation-input'
                    : 'validation-input input-normal'
                }`}
                placeholder='Financial Segment String'
                value={account.financialSegmentString}
                onChange={e => {
                  const updateAccountSilent = isCredit
                    ? this.updateCreditAccountSilent
                    : this.updateDebitAccountSilent;
                  updateAccountSilent(
                    index,
                    'financialSegmentString',
                    e.target.value
                  );
                }}
                onBlur={() =>
                  this.handleFinancialSegmentBlur(index, direction, account)
                }
                maxLength={70}
                required
                disabled={account.isValidating}
              />
              {account.isValidating && (
                <div className='input-group-text validation-spinner-container'>
                  <div
                    className='spinner-border spinner-border-sm validation-spinner'
                    role='status'
                  >
                    <span className='visually-hidden'>Validating...</span>
                  </div>
                </div>
              )}
              <button
                type='button'
                className='btn btn-outline-secondary'
                title='CCOA Picker'
                onClick={() =>
                  this.handleFinancialSegmentPicker(index, direction)
                }
                disabled={account.isValidating}
              >
                <i className='fas fa-search'></i>
              </button>
            </div>
          </td>
          <td className='cell-amount'>
            <CurrencyControl
              value={account.amount}
              onChange={value => updateAccount(index, 'amount', value)}
              isInvalid={account.amount <= 0}
            />
          </td>
          <td className='cell-percentage'>
            <NumberControl
              value={account.percentage}
              onChange={value => updateAccount(index, 'percentage', value)}
              min={0}
              max={100}
              step={0.01}
              placeholder='0.00'
            />
          </td>
          <td className='cell-actions'>
            {canRemove && (
              <button
                className='btn-link btn-invoice-delete'
                onClick={() => removeAccount(index)}
              >
                <i className='fas fa-trash-alt' />
              </button>
            )}
          </td>
        </tr>
        <tr>
          <td colSpan={4}>
            <input
              type='text'
              className='form-control'
              placeholder='Notes (optional)'
              value={account.notes}
              onChange={e => updateAccount(index, 'notes', e.target.value)}
            />
            {this.renderValidationMessages(account)}
          </td>
        </tr>
        {!isLastAccount && (
          <tr>
            <td colSpan={4} className='row-separator'></td>
          </tr>
        )}
      </React.Fragment>
    );
  };

  private renderAccountSection = (
    title: string,
    accounts: InvoiceRechargeItem[],
    direction: 'Credit' | 'Debit',
    onAdd: () => void
  ) => {
    const total = this.calculateTotal(accounts);
    const hasInvalidAmounts = accounts.some(account => account.amount <= 0);
    const isTotalValid =
      direction === 'Credit'
        ? accounts.length > 0 &&
          Math.abs(total - this.props.invoiceTotal) < 0.01
        : accounts.length === 0 ||
          Math.abs(total - this.props.invoiceTotal) < 0.01;

    const isValid = isTotalValid && !hasInvalidAmounts;

    return (
      <div className='mb-4'>
        <h4>{title}</h4>
        <div className='table-responsive'>
          <table className='table table-sm invoice-table recharge-table'>
            <thead>
              <tr>
                <th className='col-financial-segment'>
                  Financial Segment String *
                </th>
                <th className='col-amount'>Amount *</th>
                <th className='col-percentage'>Percentage</th>
                <th className='col-actions'></th>
              </tr>
            </thead>
            <tbody>
              {accounts.map((account, index) =>
                this.renderAccountRow(account, index, direction, accounts)
              )}
            </tbody>
          </table>
        </div>

        <div className='d-flex justify-content-between align-items-center'>
          <button
            type='button'
            className='btn btn-sm btn-outline-primary'
            onClick={onAdd}
          >
            Add {title.slice(0, -1)} Account
          </button>

          <div
            className={`text-end ${isValid ? 'text-success' : 'text-danger'}`}
          >
            <strong>Total: ${total.toFixed(2)}</strong>
            {direction === 'Credit' && !isTotalValid && (
              <div className='small text-danger'>
                Must equal invoice total: ${this.props.invoiceTotal.toFixed(2)}
              </div>
            )}
            {direction === 'Debit' && accounts.length > 0 && !isTotalValid && (
              <div className='small text-danger'>
                Must equal invoice total: ${this.props.invoiceTotal.toFixed(2)}
              </div>
            )}
            {hasInvalidAmounts && (
              <div className='small text-danger'>
                All amounts must be greater than zero
              </div>
            )}
          </div>
        </div>
      </div>
    );
  };

  // Public method to check if there are any chart string validation errors
  public hasValidationErrors = (): boolean => {
    const allAccounts = [
      ...this.state.creditAccounts,
      ...this.state.debitAccounts
    ];

    return allAccounts.some(
      account =>
        account.hasValidationError ||
        (account.validationResult && !account.validationResult.isValid)
    );
  };

  public render() {
    const { creditAccounts, debitAccounts } = this.state;

    return (
      <div className='recharge-accounts-control'>
        <h3>Recharge Account Information</h3>

        {this.renderAccountSection(
          'Credits',
          creditAccounts,
          'Credit',
          this.addCreditAccount
        )}

        {this.renderAccountSection(
          'Debits (Optional)',
          debitAccounts,
          'Debit',
          this.addDebitAccount
        )}

        <div className='alert alert-info'>
          <strong>Note:</strong> Credit accounts are required and must total the
          invoice amount. All amounts must be greater than zero. Debit accounts
          are optional, but if entered, must also total the invoice amount and
          have amounts greater than zero.
        </div>
      </div>
    );
  }
}
