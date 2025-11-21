import * as React from 'react';

import { Coupon } from '../models/Coupon';
import { InvoiceDiscount } from '../models/InvoiceDiscount';
import { InvoiceItem } from '../models/InvoiceItem';

import {
  calculateDiscount,
  calculateSubTotal,
  calculateTaxAmount,
  calculateTotal
} from '../helpers/calculations';

import Checkbox from './checkbox';
import CurrencyControl from './currencyControl';
import DiscountInput from './discountInput';
import NumberControl from './numberControl';
import TaxInput from './taxInput';

interface IProps {
  coupons: Coupon[];
  items: InvoiceItem[];
  discount: InvoiceDiscount;
  taxPercent: number;
  invoiceType?: string;
  onItemsChange: (value: InvoiceItem[]) => void;
  onDiscountChange: (value: InvoiceDiscount) => void;
  onTaxPercentChange: (value: number) => void;
}

interface IState {
  items: {
    byId: number[];
    byHash: {
      [key: number]: InvoiceItem;
    };
  };
}

export default class EditItemsTable extends React.Component<IProps, IState> {
  constructor(props: IProps) {
    super(props);

    // map array to object
    const items: IState['items'] = {
      byHash: {},
      byId: []
    };

    props.items.forEach((item, index) => {
      const id = item.id;
      items.byId.push(id);
      items.byHash[id] = item;
    });

    this.state = {
      items
    };
  }

  public componentWillReceiveProps(nextProps: IProps) {
    const items: IState['items'] = {
      byHash: {},
      byId: []
    };

    nextProps.items.forEach((item, index) => {
      const id = item.id;
      items.byId.push(id);
      items.byHash[id] = item;
    });

    this.setState({ items });
  }

  public render() {
    const { coupons, discount, taxPercent, invoiceType } = this.props;
    const { items } = this.state;

    const itemsArray = items.byId.map(id => items.byHash[id]);

    const discountCalc = calculateDiscount(itemsArray, discount);
    const subtotalCalc = calculateSubTotal(itemsArray);
    const taxCalc = calculateTaxAmount(itemsArray, discount, taxPercent);
    const totalCalc = calculateTotal(itemsArray, discount, taxPercent);

    // Hide discount and tax sections for Recharge invoices
    const isRechargeInvoice = invoiceType === 'Recharge';

    return (
      <div className='table-responsive'>
        <table className='table invoice-table'>
          <thead>
            <tr>
              <th>Description</th>
              <th>
                {!!taxPercent && (
                  <div>
                    <span className='me-2'>Tax Exempt</span>
                    <span>
                      <i className='fas fa-info-circle' />
                    </span>
                  </div>
                )}
              </th>
              <th>Qty</th>
              <th className='text-center'>Price</th>
              <th>Amount</th>
              <th />
            </tr>
          </thead>
          <tbody className='tbody-invoice-details'>
            {items.byId.map(id => this.renderItem(id, items.byHash[id]))}
          </tbody>
          <tbody>
            <tr>
              <td>
                <button
                  className='btn btn-primary'
                  onClick={this.createNewItem}
                >
                  <i className='fas fa-plus' />
                  Add another item
                </button>
              </td>
              <td />
              <td>Subtotal</td>
              <td />
              <td>${subtotalCalc.toFixed(2)}</td>
              <td />
            </tr>
            {!isRechargeInvoice && (
              <tr className='align-text-top'>
                <td />
                <td />
                <td>Discount</td>
                <td>
                  <DiscountInput
                    coupons={coupons}
                    discount={discount}
                    onChange={v => this.onDiscountChange(v)}
                  />
                </td>
                <td>
                  {discount.hasDiscount && (
                    <span>-${discountCalc.toFixed(2)}</span>
                  )}
                </td>
                <td>
                  {discount.hasDiscount && (
                    <button
                      className='btn btn-danger'
                      onClick={this.removeDiscount}
                    >
                      <i className='fas fa-times btn-icon' />
                    </button>
                  )}
                </td>
              </tr>
            )}
            {!isRechargeInvoice && (
              <tr>
                <td />
                <td />
                <td>
                  <span>Tax</span>
                  <span className='ms-2'>
                    <a
                      href='https://www.taxjar.com/sales-tax-calculator/'
                      target='_blank'
                      rel='noopener noreferrer'
                    >
                      <i className='fas fa-search' />
                    </a>
                  </span>
                </td>
                <td>
                  <TaxInput
                    value={taxPercent * 100}
                    onChange={v => this.onTaxPercentChange(v)}
                  />
                </td>
                <td>{!!taxPercent && <span>${taxCalc.toFixed(2)}</span>}</td>
                <td />
              </tr>
            )}
          </tbody>
          <tfoot>
            <tr>
              <td />
              <td />
              <td>Total</td>
              <td />
              <td>${totalCalc.toFixed(2)}</td>
              <td />
            </tr>
          </tfoot>
        </table>
        {totalCalc > 99999.99 && (
          <div className='text-center alert-warning'>
            The max amount the credit card will process is $99,999.99. Please
            use multiple invoices.
          </div>
        )}
      </div>
    );
  }

  private renderItem(id: number, item: InvoiceItem) {
    const { taxPercent } = this.props;
    const { description, quantity, amount, taxExempt } = item;

    return (
      <tr key={id}>
        <td colSpan={2}>
          <div className='input-group'>
            <input
              type='text'
              className='form-control'
              placeholder=''
              value={description}
              onChange={e => {
                this.updateItemProperty(id, 'description', e.target.value);
              }}
              required={true}
              maxLength={500}
            />
            {taxPercent > 0 && (
              <div className='input-group-text'>
                <Checkbox
                  checked={taxExempt}
                  onChange={value => {
                    this.updateItemProperty(id, 'taxExempt', value);
                  }}
                />
              </div>
            )}
          </div>
          <div className='invalid-feedback'>Description required</div>
        </td>
        <td>
          <NumberControl
            min={0.01}
            step={0.01}
            placeholder='0'
            value={quantity}
            onChange={value => {
              this.updateItemProperty(id, 'quantity', value);
            }}
            required={true}
          />
          <div className='invalid-feedback text-center'>Quantity required</div>
        </td>
        <td>
          <div className='input-group'>
            <div className='input-group-text'>
              <i className='fas fa-dollar-sign' />
            </div>
            <CurrencyControl
              value={amount}
              onChange={v => {
                this.updateItemProperty(id, 'amount', v);
              }}
            />
            <div className='invalid-feedback text-center ms-4'>
              Price required
            </div>
          </div>
        </td>
        <td>${(quantity * amount).toFixed(2)}</td>
        <td>
          <button
            className='btn btn-danger btn-icon'
            onClick={() => this.removeItem(id)}
          >
            <i className='fas fa-trash-alt' />
          </button>
        </td>
      </tr>
    );
  }

  private createNewItem = () => {
    const items = this.state.items;

    const maxId = Math.max(...items.byId);
    const newId = maxId + 1;

    const newItems: IState['items'] = {
      byHash: {
        ...items.byHash,
        [newId]: {
          amount: 0,
          description: '',
          id: newId,
          quantity: 0,
          taxExempt: false,
          total: 0
        }
      },
      byId: [...items.byId, newId]
    };

    this.onItemsChange(newItems);
  };

  private removeItem = (id: number) => {
    const items = this.state.items;
    const newHash = { ...items.byHash };
    delete newHash[id];

    const newItems = {
      byHash: newHash,
      byId: items.byId.filter(i => i !== id)
    };

    // if this would set the list empty, add an empty one back
    if (newItems.byId.length < 1) {
      const newId = 1;

      newItems.byHash[newId] = {
        amount: 0,
        description: '',
        id: newId,
        quantity: 0,
        taxExempt: false,
        total: 0
      };

      newItems.byId.push(newId);
    }

    this.onItemsChange(newItems);
  };

  private updateItem = (id: number, item: InvoiceItem) => {
    const items = this.state.items;
    const newHash = { ...items.byHash };
    newHash[id] = item;

    const newItems = {
      byHash: newHash,
      byId: items.byId
    };

    this.onItemsChange(newItems);
  };

  private updateItemProperty = (id: number, name: string, value) => {
    const item = {
      ...this.state.items.byHash[id]
    };
    item.total = item.amount * item.quantity;

    item[name] = value;
    this.updateItem(id, item);
  };

  private onItemsChange = (newItems: IState['items']) => {
    const itemArray = newItems.byId.map(i => newItems.byHash[i]);
    this.props.onItemsChange(itemArray);
  };

  private onDiscountChange = (value: InvoiceDiscount) => {
    this.props.onDiscountChange(value);
  };

  private removeDiscount = () => {
    this.props.onDiscountChange({
      hasDiscount: false
    });
  };

  private onTaxPercentChange = (value: number) => {
    // pass up the actual rate
    this.props.onTaxPercentChange(value / 100);
  };
}
