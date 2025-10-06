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
    id,
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

  private renderAccountRow = (
    account: InvoiceRechargeItem,
    index: number,
    direction: 'Credit' | 'Debit'
  ) => {
    const isCredit = direction === 'Credit';
    const updateAccount = isCredit
      ? this.updateCreditAccount
      : this.updateDebitAccount;
    const removeAccount = isCredit
      ? this.removeCreditAccount
      : this.removeDebitAccount;
    const canRemove = isCredit ? this.state.creditAccounts.length > 1 : true;

    return (
      <React.Fragment key={account.id}>
        <tr>
          <td style={{ width: '60%' }}>
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
          </td>
          <td style={{ width: '10%' }}>
            <CurrencyControl
              value={account.amount}
              onChange={value => updateAccount(index, 'amount', value)}
            />
          </td>
          <td style={{ width: '10%' }}>
            <NumberControl
              value={account.percentage}
              onChange={value => updateAccount(index, 'percentage', value)}
              min={0}
              max={100}
              step={0.01}
              placeholder='0.00'
            />
          </td>
          <td style={{ width: '20%' }}>
            {canRemove && (
              <button
                type='button'
                className='btn btn-sm btn-outline-danger'
                onClick={() => removeAccount(index)}
              >
                Remove
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
    const isValid =
      direction === 'Credit'
        ? accounts.length > 0 &&
          Math.abs(total - this.props.invoiceTotal) < 0.01
        : accounts.length === 0 ||
          Math.abs(total - this.props.invoiceTotal) < 0.01;

    return (
      <div className='mb-4'>
        <h4>{title}</h4>
        <div className='table-responsive'>
          <table className='table table-sm'>
            <thead>
              <tr>
                <th style={{ width: '50%' }}>Financial Segment String *</th>
                <th style={{ width: '15%' }}>Amount *</th>
                <th style={{ width: '15%' }}>Percentage</th>
                <th style={{ width: '20%' }}>Actions</th>
              </tr>
            </thead>
            <tbody>
              {accounts.map((account, index) =>
                this.renderAccountRow(account, index, direction)
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
            {direction === 'Credit' && !isValid && (
              <div className='small text-danger'>
                Must equal invoice total: ${this.props.invoiceTotal.toFixed(2)}
              </div>
            )}
            {direction === 'Debit' && accounts.length > 0 && !isValid && (
              <div className='small text-danger'>
                Must equal invoice total: ${this.props.invoiceTotal.toFixed(2)}
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
          invoice amount. Debit accounts are optional, but if entered, must also
          total the invoice amount.
        </div>
      </div>
    );
  }
}
