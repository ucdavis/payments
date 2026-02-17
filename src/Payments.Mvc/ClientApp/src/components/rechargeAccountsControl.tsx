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
  showCreditAccounts: boolean;
  onValidationComplete?: () => void;
  fromApprove?: boolean; // When true, only chart string is editable, amount/percentage/add button disabled
}

interface IState {
  creditAccounts: InvoiceRechargeItem[];
  debitAccounts: InvoiceRechargeItem[];
  nextId: number;
  isInternalUpdate: boolean; // Flag to track internal updates vs external prop changes
  isCalculatingAmountPercentage: boolean; // Flag to prevent circular updates between amount and percentage
}

export default class RechargeAccountsControl extends React.Component<
  IProps,
  IState
> {
  private inputRefs: {
    [key: string]: HTMLInputElement | null;
  } = {};

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

    // Ensure we have at least one account of the required type
    if (props.showCreditAccounts && creditAccounts.length === 0) {
      const newAccount = this.createNewAccount('Credit', 1);
      creditAccounts.push({
        ...newAccount,
        validationResult: undefined,
        isValidating: false,
        hasValidationError: false,
        skipNextValidation: false
      });
    } else if (!props.showCreditAccounts && debitAccounts.length === 0) {
      const newAccount = this.createNewAccount('Debit', 1);
      debitAccounts.push({
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
      isInternalUpdate: false,
      isCalculatingAmountPercentage: false
    };
  }

  async componentDidMount() {
    // Validate existing financial segment strings after component mounts
    // Only validate if there are accounts with existing financial segment strings
    const hasExistingData = this.props.rechargeAccounts.some(
      account =>
        account.financialSegmentString && account.financialSegmentString.trim()
    );

    if (hasExistingData) {
      await this.validateExistingAccounts();
      // Also validate credit totals after initial render
      this.setCreditTotalValidityOnLastAmount();
      // Also validate debit totals after initial render
      this.setDebitTotalValidityOnLastAmount();
    } else {
      // Even if no existing data, validate credit totals after initial render
      this.setCreditTotalValidityOnLastAmount();
      this.setDebitTotalValidityOnLastAmount();
      // Notify parent that validation is complete (no data to validate)
      if (this.props.onValidationComplete) {
        this.props.onValidationComplete();
      }
    }
  }

  componentDidUpdate(prevProps: IProps) {
    // If the invoiceTotal changes, recalculate percentages based on current amounts
    if (
      prevProps.invoiceTotal !== this.props.invoiceTotal &&
      this.props.invoiceTotal > 0
    ) {
      console.log(
        `Invoice total changed from ${prevProps.invoiceTotal} to ${this.props.invoiceTotal}, recalculating percentages`
      );
      this.recalculatePercentagesFromAmounts();
    }

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

        // Ensure we have at least one account of the required type
        if (this.props.showCreditAccounts && creditAccounts.length === 0) {
          const newAccount = this.createNewAccount('Credit', this.state.nextId);
          creditAccounts.push({
            ...newAccount,
            validationResult: undefined,
            isValidating: false,
            hasValidationError: false,
            skipNextValidation: false
          });
        } else if (
          !this.props.showCreditAccounts &&
          debitAccounts.length === 0
        ) {
          const newAccount = this.createNewAccount('Debit', this.state.nextId);
          debitAccounts.push({
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
              // Schedule validation after setState completes
              this.scheduleValidation();
            } else {
              // Even if no data to validate, update credit totals
              this.scheduleTotalValidation();
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

  // Helper method to set custom validity on chart string inputs
  private setChartStringCustomValidity = (
    index: number,
    direction: 'Credit' | 'Debit',
    isValid: boolean
  ) => {
    const inputKey = `${direction.toLowerCase()}-${index}-chart`;
    const element = this.inputRefs[inputKey];

    if (element) {
      if (isValid) {
        element.setCustomValidity('');
      } else {
        element.setCustomValidity('This has errors');
      }
    }
  };

  // Helper method to set custom validity on amount inputs for total validation
  private setCreditTotalValidityOnLastAmount = () => {
    this.setCreditTotalValidityOnAllAmounts();
  };

  // Helper method to set custom validity on all credit amount inputs for total validation
  private setCreditTotalValidityOnAllAmounts = () => {
    if (this.state.creditAccounts.length === 0) {
      return;
    }

    const total = this.calculateTotal(this.state.creditAccounts);
    const isTotalValid = this.isTotalEqual(total, this.props.invoiceTotal);
    const isValidOrNoTotal = isTotalValid || this.props.invoiceTotal <= 0;

    // Update validation on all credit amount fields
    for (let i = 0; i < this.state.creditAccounts.length; i++) {
      const refKey = `credit-${i}-amount-ref`;
      const inputElement = this.inputRefs[refKey];

      if (inputElement) {
        if (isValidOrNoTotal) {
          inputElement.setCustomValidity('');
        } else {
          inputElement.setCustomValidity(
            'Credit amounts must total the invoice amount'
          );
        }
      }
    }
  };

  // Helper method to set custom validity on debit amount inputs for total validation
  private setDebitTotalValidityOnLastAmount = () => {
    this.setDebitTotalValidityOnAllAmounts();
  };

  // Helper method to set custom validity on all debit amount inputs for total validation
  private setDebitTotalValidityOnAllAmounts = () => {
    if (this.state.debitAccounts.length === 0) {
      return; // No debit accounts, so no validation needed
    }

    const total = this.calculateTotal(this.state.debitAccounts);
    const isTotalValid = this.isTotalEqual(total, this.props.invoiceTotal);
    const isValidOrNoTotal = isTotalValid || this.props.invoiceTotal <= 0;

    // Update validation on all debit amount fields
    for (let i = 0; i < this.state.debitAccounts.length; i++) {
      const refKey = `debit-${i}-amount-ref`;
      const inputElement = this.inputRefs[refKey];

      if (inputElement) {
        if (isValidOrNoTotal) {
          inputElement.setCustomValidity('');
        } else {
          inputElement.setCustomValidity(
            'Debit amounts must total the invoice amount'
          );
        }
      }
    }
  };

  private validateChartString = async (
    chartString: string,
    direction: 'Credit' | 'Debit'
  ): Promise<AccountValidationModel | null> => {
    if (!chartString.trim()) {
      return null;
    }

    try {
      const directionValue = direction === 'Credit' ? 0 : 1;
      // Add checkApprover param when fromApprove is true and direction is Debit
      const checkApprover = this.props.fromApprove && direction === 'Debit';
      const response = await fetch(
        `/api/recharge/validate?chartString=${encodeURIComponent(
          chartString
        )}&direction=${directionValue}&checkApprover=${checkApprover}`,
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
  ): Promise<void> => {
    console.log(
      `Starting validation for ${direction} account ${index} with chartString: ${chartString}`
    );

    const accounts =
      direction === 'Credit'
        ? this.state.creditAccounts
        : this.state.debitAccounts;

    // Create updated account with isValidating flag
    const updatedAccountValidating = {
      ...accounts[index],
      isValidating: true,
      hasValidationError: false
    };

    // Update state to show validating and notify parent so it knows validation is in progress
    await this.updateAccountState(
      direction,
      index,
      updatedAccountValidating,
      true
    );

    let valueChanged = false;

    try {
      const validationResult = await this.validateChartString(
        chartString,
        direction
      );

      // Get the latest account state after validation
      const currentAccounts =
        direction === 'Credit'
          ? this.state.creditAccounts
          : this.state.debitAccounts;
      const currentAccount = currentAccounts[index];

      let updatedAccount = {
        ...currentAccount,
        isValidating: false
      };

      if (validationResult) {
        // Update the chart string if validation returned a corrected value
        if (validationResult.chartString !== chartString) {
          console.log(
            `Validation corrected chart string from "${chartString}" to "${validationResult.chartString}"`
          );

          updatedAccount = {
            ...updatedAccount,
            financialSegmentString: validationResult.chartString,
            skipNextValidation: true,
            validationResult: validationResult,
            hasValidationError: !validationResult.isValid
          };
          valueChanged = true;
        } else {
          updatedAccount = {
            ...updatedAccount,
            validationResult: validationResult,
            hasValidationError: !validationResult.isValid
          };
        }

        // Set custom validity on the input element
        this.setChartStringCustomValidity(
          index,
          direction,
          validationResult.isValid
        );
      } else {
        updatedAccount = {
          ...updatedAccount,
          validationResult: undefined,
          hasValidationError: false
        };

        // Clear custom validity for empty strings
        this.setChartStringCustomValidity(index, direction, true);
      }

      // Update state with all changes at once and notify parent
      await this.updateAccountState(direction, index, updatedAccount, true);
    } catch (error) {
      console.error('Validation error:', error);

      const currentAccounts =
        direction === 'Credit'
          ? this.state.creditAccounts
          : this.state.debitAccounts;
      const updatedAccount = {
        ...currentAccounts[index],
        isValidating: false,
        hasValidationError: true
      };

      // Set custom validity for validation errors
      this.setChartStringCustomValidity(index, direction, false);

      // Update state and notify parent
      await this.updateAccountState(direction, index, updatedAccount, true);
    }

    console.log(
      `Validation completed for ${direction} account ${index}, value changed: ${valueChanged}`
    );
  };

  private handleFinancialSegmentBlur = (
    index: number,
    direction: 'Credit' | 'Debit'
  ) => {
    // Get the current account from state, not from render-time props
    const accounts =
      direction === 'Credit'
        ? this.state.creditAccounts
        : this.state.debitAccounts;
    const account = accounts[index];

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
      // Clear custom validity for empty fields
      this.setChartStringCustomValidity(index, direction, true);
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
        await this.updateCreditAccountSilent(i, 'skipNextValidation', true);
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
        await this.updateDebitAccountSilent(i, 'skipNextValidation', true);
        await this.handleChartStringValidation(
          i,
          'Debit',
          account.financialSegmentString
        );
      }
    }

    // All validation is complete - now update parent and notify
    return new Promise<void>(resolve => {
      // Set isInternalUpdate flag to prevent re-validation in componentDidUpdate
      this.setState({ isInternalUpdate: true }, () => {
        // Update parent with validated accounts
        this.updateAccounts();

        // Notify parent that validation is complete
        if (this.props.onValidationComplete) {
          this.props.onValidationComplete();
        }

        resolve();
      });
    });
  };

  // Helper method to schedule full validation after state updates
  private scheduleValidation = async () => {
    await this.validateExistingAccounts();
    this.setCreditTotalValidityOnLastAmount();
    this.setDebitTotalValidityOnLastAmount();
  };

  // Helper method to schedule only total validation after state updates
  private scheduleTotalValidation = () => {
    this.setCreditTotalValidityOnLastAmount();
    this.setDebitTotalValidityOnLastAmount();
  };

  private recalculatePercentagesFromAmounts = () => {
    if (this.props.invoiceTotal <= 0) {
      return;
    }

    // Recalculate percentages for credit accounts
    const updatedCreditAccounts = this.state.creditAccounts.map(account => ({
      ...account,
      percentage:
        account.amount > 0
          ? parseFloat(
              ((account.amount / this.props.invoiceTotal) * 100).toFixed(2)
            )
          : 0
    }));

    // Recalculate percentages for debit accounts
    const updatedDebitAccounts = this.state.debitAccounts.map(account => ({
      ...account,
      percentage:
        account.amount > 0
          ? parseFloat(
              ((account.amount / this.props.invoiceTotal) * 100).toFixed(2)
            )
          : 0
    }));

    this.setState(
      {
        creditAccounts: updatedCreditAccounts,
        debitAccounts: updatedDebitAccounts,
        isInternalUpdate: true
      },
      () => {
        this.updateAccounts();
        // Update credit total validity when invoice total changes
        this.scheduleTotalValidation();
      }
    );
  };

  private updateAccounts = () => {
    const allAccounts = [
      ...this.state.creditAccounts,
      ...this.state.debitAccounts
    ];
    this.props.onChange(allAccounts);
  };

  // Helper method to update a single account's state and optionally notify parent
  private updateAccountState = (
    direction: 'Credit' | 'Debit',
    index: number,
    updatedAccount: InvoiceRechargeItem,
    notifyParent: boolean
  ): Promise<void> => {
    return new Promise(resolve => {
      if (direction === 'Credit') {
        this.setState(
          prevState => {
            const updated = [...prevState.creditAccounts];
            updated[index] = updatedAccount;
            return {
              creditAccounts: updated,
              isInternalUpdate: !notifyParent
            };
          },
          () => {
            if (notifyParent) {
              this.updateAccounts();
            }
            resolve();
          }
        );
      } else {
        this.setState(
          prevState => {
            const updated = [...prevState.debitAccounts];
            updated[index] = updatedAccount;
            return {
              debitAccounts: updated,
              isInternalUpdate: !notifyParent
            };
          },
          () => {
            if (notifyParent) {
              this.updateAccounts();
            }
            resolve();
          }
        );
      }
    });
  };

  // Silent update methods that don't trigger parent onChange (for validation updates)
  // These now return Promises to allow waiting for state updates to complete
  private updateCreditAccountSilent = (
    index: number,
    field: keyof InvoiceRechargeItem,
    value: any
  ): Promise<void> => {
    return new Promise(resolve => {
      this.setState(prevState => {
        const updated = [...prevState.creditAccounts];
        updated[index] = { ...updated[index], [field]: value };
        return { creditAccounts: updated };
      }, resolve);
    });
  };

  private updateDebitAccountSilent = (
    index: number,
    field: keyof InvoiceRechargeItem,
    value: any
  ): Promise<void> => {
    return new Promise(resolve => {
      this.setState(prevState => {
        const updated = [...prevState.debitAccounts];
        updated[index] = { ...updated[index], [field]: value };
        return { debitAccounts: updated };
      }, resolve);
    });
  };

  private updateCreditAccount = (
    index: number,
    field: keyof InvoiceRechargeItem,
    value: any
  ) => {
    // Prevent circular updates between amount and percentage
    if (this.state.isCalculatingAmountPercentage) {
      return;
    }

    const { creditAccounts } = this.state;
    const updatedAccounts = [...creditAccounts];
    const currentAccount = updatedAccounts[index];

    // Update the field that was changed
    updatedAccounts[index] = { ...currentAccount, [field]: value };

    // Calculate the corresponding field if amount or percentage was changed
    if (
      (field === 'amount' || field === 'percentage') &&
      this.props.invoiceTotal > 0
    ) {
      this.setState({ isCalculatingAmountPercentage: true }, () => {
        if (field === 'amount' && value > 0) {
          // Calculate percentage when amount changes
          const percentage = (value / this.props.invoiceTotal) * 100;
          updatedAccounts[index] = {
            ...updatedAccounts[index],
            percentage: parseFloat(percentage.toFixed(2))
          };
        } else if (field === 'percentage' && value >= 0 && value <= 100) {
          // Calculate amount when percentage changes
          const amount = (value / 100) * this.props.invoiceTotal;
          updatedAccounts[index] = {
            ...updatedAccounts[index],
            amount: parseFloat(amount.toFixed(2))
          };
        }

        this.setState(
          {
            creditAccounts: updatedAccounts,
            isInternalUpdate: true,
            isCalculatingAmountPercentage: false
          },
          () => {
            this.updateAccounts();
            // Update credit total validity when amounts change
            this.setCreditTotalValidityOnLastAmount();
          }
        );
      });
    } else {
      this.setState(
        {
          creditAccounts: updatedAccounts,
          isInternalUpdate: true
        },
        () => {
          this.updateAccounts();
          // Update credit total validity when amounts change
          this.setCreditTotalValidityOnLastAmount();
        }
      );
    }
  };

  private updateDebitAccount = (
    index: number,
    field: keyof InvoiceRechargeItem,
    value: any
  ) => {
    // Prevent circular updates between amount and percentage
    if (this.state.isCalculatingAmountPercentage) {
      return;
    }

    const { debitAccounts } = this.state;
    const updatedAccounts = [...debitAccounts];
    const currentAccount = updatedAccounts[index];

    // Update the field that was changed
    updatedAccounts[index] = { ...currentAccount, [field]: value };

    // Calculate the corresponding field if amount or percentage was changed
    if (
      (field === 'amount' || field === 'percentage') &&
      this.props.invoiceTotal > 0
    ) {
      this.setState({ isCalculatingAmountPercentage: true }, () => {
        if (field === 'amount' && value > 0) {
          // Calculate percentage when amount changes
          const percentage = (value / this.props.invoiceTotal) * 100;
          updatedAccounts[index] = {
            ...updatedAccounts[index],
            percentage: parseFloat(percentage.toFixed(2))
          };
        } else if (field === 'percentage' && value >= 0 && value <= 100) {
          // Calculate amount when percentage changes
          const amount = (value / 100) * this.props.invoiceTotal;
          updatedAccounts[index] = {
            ...updatedAccounts[index],
            amount: parseFloat(amount.toFixed(2))
          };
        }

        this.setState(
          {
            debitAccounts: updatedAccounts,
            isInternalUpdate: true,
            isCalculatingAmountPercentage: false
          },
          () => {
            this.updateAccounts();
            // Update debit total validity when amounts change
            this.setDebitTotalValidityOnLastAmount();
          }
        );
      });
    } else {
      this.setState(
        {
          debitAccounts: updatedAccounts,
          isInternalUpdate: true
        },
        () => {
          this.updateAccounts();
          // Update debit total validity when amounts change
          this.setDebitTotalValidityOnLastAmount();
        }
      );
    }
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
      () => {
        this.updateAccounts();
        // Clear custom validity for new account and update totals
        this.setChartStringCustomValidity(
          creditAccounts.length,
          'Credit',
          true
        );
        this.setCreditTotalValidityOnLastAmount();
      }
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
      () => {
        this.updateAccounts();
        // Clear custom validity for new account and update totals
        this.setChartStringCustomValidity(debitAccounts.length, 'Debit', true);
        this.setDebitTotalValidityOnLastAmount();
      }
    );
  };

  private removeCreditAccount = (index: number) => {
    const { creditAccounts } = this.state;
    // Must have at least one credit account when showCreditAccounts is true
    if (creditAccounts.length > 1 || !this.props.showCreditAccounts) {
      const updatedAccounts = creditAccounts.filter((_, i) => i !== index);
      this.setState(
        {
          creditAccounts: updatedAccounts,
          isInternalUpdate: true
        },
        () => {
          this.updateAccounts();
          // Update credit total validity when accounts are removed
          this.setCreditTotalValidityOnLastAmount();
        }
      );
    }
  };

  private removeDebitAccount = (index: number) => {
    const { debitAccounts } = this.state;
    // Must have at least one debit account when showCreditAccounts is false
    if (debitAccounts.length > 1 || this.props.showCreditAccounts) {
      const updatedAccounts = debitAccounts.filter((_, i) => i !== index);
      this.setState(
        {
          debitAccounts: updatedAccounts,
          isInternalUpdate: true
        },
        () => {
          this.updateAccounts();
          // Update debit total validity when accounts are removed
          this.setDebitTotalValidityOnLastAmount();
        }
      );
    }
  };

  private calculateTotal = (accounts: InvoiceRechargeItem[]): number => {
    const total = accounts.reduce((sum, account) => sum + account.amount, 0);
    // Round to 2 decimal places to handle floating point precision issues
    return parseFloat(total.toFixed(2));
  };

  // Helper method to compare monetary amounts with proper rounding
  private isTotalEqual = (total: number, expected: number): boolean => {
    // Round both values to 2 decimal places before comparison
    const roundedTotal = parseFloat(total.toFixed(2));
    const roundedExpected = parseFloat(expected.toFixed(2));
    return roundedTotal === roundedExpected;
  };

  private renderValidationMessages = (account: InvoiceRechargeItem) => {
    if (!account.validationResult) {
      return null;
    }

    const { validationResult } = account;
    const hasMessages = validationResult.messages.length > 0;
    const hasWarnings = validationResult.warnings.length > 0;
    const isValid = validationResult.isValid;
    const hasValidSegmentString =
      account.financialSegmentString &&
      account.financialSegmentString.trim().length > 0;

    // If no validation result to show, return null
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
        {isValid && !hasMessages && hasValidSegmentString && (
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
          await updateAccountSilent(index, 'skipNextValidation', true);

          // Update the field value and notify parent.
          const currentAccounts =
            direction === 'Credit'
              ? this.state.creditAccounts
              : this.state.debitAccounts;
          const currentAccount = currentAccounts[index];
          const updatedAccount = {
            ...currentAccount,
            financialSegmentString: chart.data,
            skipNextValidation: true
          };

          await this.updateAccountState(direction, index, updatedAccount, true);

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

    // Determine if this account can be removed based on mode and count
    const canRemove =
      !this.props.fromApprove &&
      (isCredit
        ? this.state.creditAccounts.length > 1 || !this.props.showCreditAccounts
        : this.state.debitAccounts.length > 1 || this.props.showCreditAccounts);

    const isLastAccount = index === accounts.length - 1;

    return (
      <React.Fragment key={`${direction}-${index}`}>
        <tr>
          <td className='cell-financial-segment'>
            <label className='form-label'>Financial Segment String *</label>
            <div className='input-group'>
              <input
                ref={el => {
                  this.inputRefs[
                    `${direction.toLowerCase()}-${index}-chart`
                  ] = el;
                }}
                type='text'
                className={`form-control ${
                  account.isValidating
                    ? 'validation-input'
                    : 'validation-input input-normal'
                }`}
                placeholder='Financial Segment String'
                value={account.financialSegmentString}
                onChange={e => {
                  const value = e.target.value;
                  this.setState(prevState => {
                    const accounts = isCredit
                      ? prevState.creditAccounts
                      : prevState.debitAccounts;
                    const updated = [...accounts];
                    updated[index] = {
                      ...updated[index],
                      financialSegmentString: value,
                      skipNextValidation: false // Clear skip flag when user manually types
                    };
                    return isCredit
                      ? { creditAccounts: updated }
                      : { debitAccounts: updated };
                  });

                  // Clear custom validity when user starts typing
                  if (value.trim() === '') {
                    this.setChartStringCustomValidity(index, direction, true);
                  }
                }}
                onBlur={() => this.handleFinancialSegmentBlur(index, direction)}
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
                className='btn btn-outline-primary btn-icon'
                title='CCOA Picker'
                onClick={() =>
                  this.handleFinancialSegmentPicker(index, direction)
                }
                disabled={account.isValidating}
              >
                <i className='fas fa-search'></i>
              </button>
              {account.financialSegmentString && (
                <a
                  href={`https://finjector.ucdavis.edu/details/${account.financialSegmentString}`}
                  target='_blank'
                  rel='noopener noreferrer'
                  className={`btn btn-outline-primary btn-icon ${
                    account.isValidating ? 'disabled' : ''
                  }`}
                  title='View In Finjector'
                  aria-disabled={account.isValidating}
                  onClick={e => {
                    if (account.isValidating) {
                      e.preventDefault();
                    }
                  }}
                >
                  <i className='fas fa-external-link-alt'></i>
                </a>
              )}
            </div>
          </td>
          <td className='cell-actions'>
            {canRemove && (
              <button
                className='btn btn-danger btn-icon'
                onClick={() => removeAccount(index)}
              >
                <i className='fas fa-trash-alt' />
              </button>
            )}
          </td>
        </tr>
        <tr>
          <td className='cell-amount'>
            <label className='form-label'>Amount *</label>
            <div className='input-group'>
              <div className='input-group-text'>
                <i className='fas fa-dollar-sign' />
              </div>

              <CurrencyControl
                value={account.amount}
                onChange={value => updateAccount(index, 'amount', value)}
                isInvalid={account.amount <= 0}
                disabled={this.props.fromApprove}
                inputRef={el => {
                  const refKey = `${direction.toLowerCase()}-${index}-amount-ref`;
                  this.inputRefs[refKey] = el;
                }}
              />
            </div>
          </td>
          <td className='cell-percentage'>
            <label className='form-label'>Percentage</label>
            <NumberControl
              value={account.percentage}
              onChange={value => updateAccount(index, 'percentage', value)}
              min={0}
              max={100}
              step={0.01}
              placeholder='0.00'
              disabled={this.props.fromApprove}
            />
          </td>
        </tr>
        <tr>
          <td colSpan={2}>
            <label className='form-label'>Notes</label>
            <input
              type='text'
              className='form-control'
              placeholder='Notes (optional)'
              value={account.notes}
              onChange={e => updateAccount(index, 'notes', e.target.value)}
              disabled={this.props.fromApprove}
            />
            {this.renderValidationMessages(account)}
          </td>
        </tr>
        {!isLastAccount && (
          <tr>
            <td colSpan={2} className='row-separator'></td>
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

    // Determine if this section is required based on showCreditAccounts prop
    const isRequired = this.props.showCreditAccounts
      ? direction === 'Credit' // In credit mode, only credits are required
      : direction === 'Debit'; // In debit-only mode, debits are required

    // When fromApprove is true, we don't validate totals (amounts are read-only)
    const isTotalValid = this.props.fromApprove
      ? true
      : isRequired
      ? accounts.length > 0 && this.isTotalEqual(total, this.props.invoiceTotal)
      : accounts.length === 0 ||
        this.isTotalEqual(total, this.props.invoiceTotal);

    // When fromApprove is true, we don't check amounts (they're read-only)
    const isValid = this.props.fromApprove
      ? true
      : isTotalValid && !hasInvalidAmounts;

    return (
      <div className='mb-4'>
        <h4>{title}</h4>
        {accounts?.length > 0 && (
          <>
            <div className='table-responsive'>
              <table className='table table-sm invoice-table recharge-table'>
                <tbody>
                  {accounts.map((account, index) =>
                    this.renderAccountRow(account, index, direction, accounts)
                  )}
                </tbody>
              </table>
            </div>
          </>
        )}
        <div className='d-flex justify-content-between align-items-center'>
          {!this.props.fromApprove && (
            <button
              type='button'
              className='btn btn-sm btn-outline-primary'
              onClick={onAdd}
            >
              Add {direction} Account
            </button>
          )}

          <div
            className={`text-end ${isValid ? 'text-success' : 'text-danger'}`}
          >
            <strong>Total: ${total.toFixed(2)}</strong>
            {!this.props.fromApprove && isRequired && !isTotalValid && (
              <div className='small text-danger'>
                Must equal invoice total: ${this.props.invoiceTotal.toFixed(2)}
              </div>
            )}
            {!this.props.fromApprove &&
              !isRequired &&
              accounts.length > 0 &&
              !isTotalValid && (
                <div className='small text-danger'>
                  Must equal invoice total: $
                  {this.props.invoiceTotal.toFixed(2)}
                </div>
              )}
            {!this.props.fromApprove && hasInvalidAmounts && (
              <div className='small text-danger'>
                All amounts must be greater than zero
              </div>
            )}
          </div>
        </div>
      </div>
    );
  };

  // Public method to check if any validations are currently in progress
  public isValidating = (): boolean => {
    const allAccounts = [
      ...this.state.creditAccounts,
      ...this.state.debitAccounts
    ];

    return allAccounts.some(account => account.isValidating);
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
    const { showCreditAccounts } = this.props;

    return (
      <div className='recharge-accounts-control'>
        <h3>Recharge Account Information</h3>

        {showCreditAccounts &&
          this.renderAccountSection(
            'Credits',
            creditAccounts,
            'Credit',
            this.addCreditAccount
          )}

        {showCreditAccounts &&
          this.renderAccountSection(
            'Debits (Optional)',
            debitAccounts,
            'Debit',
            this.addDebitAccount
          )}

        {!showCreditAccounts &&
          this.renderAccountSection(
            'Debits',
            debitAccounts,
            'Debit',
            this.addDebitAccount
          )}

        {!this.props.fromApprove && (
          <div className='alert alert-info'>
            <strong>Note:</strong>
            {showCreditAccounts ? (
              <>
                {' '}
                Credit accounts are required and must total the invoice amount.
                All amounts must be greater than zero. Debit accounts are
                optional, but if entered, must also total the invoice amount and
                have amounts greater than zero.
              </>
            ) : (
              <>
                {' '}
                Debit accounts are required and must total the invoice amount.
                All amounts must be greater than zero.
              </>
            )}
          </div>
        )}
      </div>
    );
  }
}
