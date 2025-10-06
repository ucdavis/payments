import * as React from 'react';

import { InvoiceRechargeItem } from '../models/InvoiceRechargeItem';

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
      direction: this.normalizeDirection(account.direction)
    }));

    const creditAccounts = normalizedAccounts.filter(
      account => account.direction === 'Credit'
    );
    const debitAccounts = normalizedAccounts.filter(
      account => account.direction === 'Debit'
    );

    // Ensure we have at least one credit account
    if (creditAccounts.length === 0) {
      creditAccounts.push(this.createNewAccount('Credit', 1));
    }

    const nextId = Math.max(...props.rechargeAccounts.map(a => a.id), 0) + 1;

    this.state = {
      creditAccounts,
      debitAccounts,
      nextId
    };
  }

  componentDidUpdate(prevProps: IProps) {
    // If the rechargeAccounts prop changes, update our state
    if (prevProps.rechargeAccounts !== this.props.rechargeAccounts) {
      const normalizedAccounts = this.props.rechargeAccounts.map(account => ({
        ...account,
        direction: this.normalizeDirection(account.direction)
      }));

      const creditAccounts = normalizedAccounts.filter(
        account => account.direction === 'Credit'
      );
      const debitAccounts = normalizedAccounts.filter(
        account => account.direction === 'Debit'
      );

      // Ensure we have at least one credit account
      if (creditAccounts.length === 0) {
        creditAccounts.push(this.createNewAccount('Credit', this.state.nextId));
      }

      this.setState({
        creditAccounts,
        debitAccounts
      });
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
    notes: ''
  });

  private updateAccounts = () => {
    const allAccounts = [
      ...this.state.creditAccounts,
      ...this.state.debitAccounts
    ];
    this.props.onChange(allAccounts);
  };

  private updateCreditAccount = (
    index: number,
    field: keyof InvoiceRechargeItem,
    value: any
  ) => {
    const { creditAccounts } = this.state;
    const updatedAccounts = [...creditAccounts];
    updatedAccounts[index] = { ...updatedAccounts[index], [field]: value };

    this.setState({ creditAccounts: updatedAccounts }, this.updateAccounts);
  };

  private updateDebitAccount = (
    index: number,
    field: keyof InvoiceRechargeItem,
    value: any
  ) => {
    const { debitAccounts } = this.state;
    const updatedAccounts = [...debitAccounts];
    updatedAccounts[index] = { ...updatedAccounts[index], [field]: value };

    this.setState({ debitAccounts: updatedAccounts }, this.updateAccounts);
  };

  private addCreditAccount = () => {
    const { creditAccounts, nextId } = this.state;
    const newAccount = this.createNewAccount('Credit', nextId);

    this.setState(
      {
        creditAccounts: [...creditAccounts, newAccount],
        nextId: nextId + 1
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
        nextId: nextId + 1
      },
      this.updateAccounts
    );
  };

  private removeCreditAccount = (index: number) => {
    const { creditAccounts } = this.state;
    if (creditAccounts.length > 1) {
      // Must have at least one credit account
      const updatedAccounts = creditAccounts.filter((_, i) => i !== index);
      this.setState({ creditAccounts: updatedAccounts }, this.updateAccounts);
    }
  };

  private removeDebitAccount = (index: number) => {
    const { debitAccounts } = this.state;
    const updatedAccounts = debitAccounts.filter((_, i) => i !== index);
    this.setState({ debitAccounts: updatedAccounts }, this.updateAccounts);
  };

  private calculateTotal = (accounts: InvoiceRechargeItem[]): number => {
    return accounts.reduce((sum, account) => sum + account.amount, 0);
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
          const updateAccount =
            direction === 'Credit'
              ? this.updateCreditAccount
              : this.updateDebitAccount;
          updateAccount(index, 'financialSegmentString', chart.data);
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
          <td style={{ width: '55%', paddingRight: '8px' }}>
            <div className='input-group'>
              <input
                type='text'
                className='form-control'
                placeholder='Financial Segment String'
                value={account.financialSegmentString}
                onChange={e =>
                  updateAccount(index, 'financialSegmentString', e.target.value)
                }
                maxLength={70}
                required
              />
              <button
                type='button'
                className='btn btn-outline-secondary'
                title='CCOA Picker'
                onClick={() =>
                  this.handleFinancialSegmentPicker(index, direction)
                }
              >
                <i className='fas fa-search'></i>
              </button>
            </div>
          </td>
          <td style={{ width: '12%', paddingLeft: '4px', paddingRight: '4px' }}>
            <CurrencyControl
              value={account.amount}
              onChange={value => updateAccount(index, 'amount', value)}
              isInvalid={account.amount <= 0}
            />
          </td>
          <td style={{ width: '12%', paddingLeft: '4px', paddingRight: '4px' }}>
            <NumberControl
              value={account.percentage}
              onChange={value => updateAccount(index, 'percentage', value)}
              min={0}
              max={100}
              step={0.01}
              placeholder='0.00'
            />
          </td>
          <td style={{ width: '6%', textAlign: 'right', paddingLeft: '8px' }}>
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
          </td>
        </tr>
        {!isLastAccount && (
          <tr>
            <td
              colSpan={4}
              style={{
                padding: '0',
                height: '8px',
                borderBottom: '2px solid #dee2e6'
              }}
            ></td>
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
          <table className='table table-sm invoice-table'>
            <thead>
              <tr>
                <th style={{ width: '55%' }}>Financial Segment String *</th>
                <th style={{ width: '12%' }}>Amount *</th>
                <th style={{ width: '12%' }}>Percentage</th>
                <th style={{ width: '6%' }}></th>
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
