import * as React from 'react';

import { uuidv4 } from '../utils/string';

import { Coupon } from '../models/Coupon';
import { InvoiceDiscount } from '../models/InvoiceDiscount';
import { InvoiceItem } from '../models/InvoiceItem';

import CurrencyControl from './currencyControl';
import DiscountInput from './discountInput';
import TaxInput from './taxInput';

interface IProps {
    coupons: Coupon[];
    items: InvoiceItem[];
    discount: InvoiceDiscount;
    taxPercent: number;
    onItemsChange: (value: InvoiceItem[]) => void;
    onDiscountChange: (value: InvoiceDiscount) => void;
    onTaxPercentChange: (value: number) => void;
}

interface IState {
    items: {
        byId: string[];
        byHash: {
            [key: number]: InvoiceItem;
        };
    };
}

export default class EditItemsTable extends React.Component<IProps, IState> {
    constructor(props: IProps) {
        super(props);

        // map array to object
        const items: IState["items"] = {
            byHash: {},
            byId: [],
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
        const items: IState["items"] = {
            byHash: {},
            byId: [],
        };

        nextProps.items.forEach((item, index) => {
            const id = item.id;
            items.byId.push(id);
            items.byHash[id] = item;
        });

        this.setState({ items });
    }

    public render() {
        const { coupons, discount, taxPercent } = this.props;
        const { items } = this.state;

        const discountCalc = this.calculateDiscount();
        const subtotalCalc = this.calculateSubTotal();
        const taxCalc = this.calculateTaxAmount();
        const totalCalc = this.calculateTotal();

        return (
            <table className="table invoice-table">
                <thead>
                    <tr>
                        <th>Description</th>
                        <th>Qty</th>
                        <th className="text-center">Price</th>
                        <th>Amount</th>
                        <th/>
                    </tr>
                </thead>
                <tbody className="tbody-invoice-details">
                    { items.byId.map((id) => this.renderItem(id, items.byHash[id])) }
                </tbody>
                <tbody>
                    <tr>
                        <td>
                            <button className="btn-plain primary-color" onClick={this.createNewItem}>
                                <i className="fas fa-plus mr-2" /> Add another item
                            </button>
                        </td>
                        <td>Subtotal</td>
                        <td />
                        <td>${ subtotalCalc.toFixed(2) }</td>
                        <td />
                    </tr>
                    <tr className="align-text-top">
                        <td />
                        <td>Discount</td>
                        <td><DiscountInput coupons={coupons} discount={discount} onChange={(v) => this.onDiscountChange(v)} /></td>
                        <td>{ 
                                (discount.hasDiscount) && 
                                <span>-${ discountCalc.toFixed(2) }</span>
                            }
                        </td>
                        <td>
                            {
                                (discount.hasDiscount) && 
                                <button className="btn-link btn-invoice-delete" onClick={this.removeDiscount}>
                                    <i className="fas fa-times" />
                                </button>
                            }
                        </td>
                    </tr>
                    <tr>
                        <td />
                        <td>Tax</td>
                        <td><TaxInput value={taxPercent * 100} onChange={(v) => this.onTaxPercentChange(v)} /></td>
                        <td>{
                                (taxPercent !== 0) &&
                                <span>${ taxCalc.toFixed(2) }</span>
                            }
                        </td>
                        <td />
                    </tr>
                </tbody>
                <tfoot>
                    <tr>
                        <td />
                        <td>Total</td>
                        <td />
                        <td>${ totalCalc.toFixed(2) }</td>
                        <td />
                    </tr>
                </tfoot>
            </table>
        );
    }

    private renderItem(id: string, item: InvoiceItem) {
        const { description, quantity, amount } = item;

        return (
            <tr key={id}>
                <td>
                    <input
                        type="text"
                        className="form-control"
                        placeholder=""
                        value={description}
                        onChange={(e) => { this.updateItemProperty(id, 'description', e.target.value) }}
                        required={true}
                    />
                    <div className="invalid-feedback">
                        Description required
                    </div>
                </td>
                <td>
                    <input
                        type="number"
                        min="0.01"
                        step="0.01"
                        className="form-control"
                        placeholder="0"
                        value={quantity}
                        onChange={(e) => { this.updateItemProperty(id, 'quantity', e.target.value) }}
                        required={true}
                    />
                    <div className="invalid-feedback text-center">
                        Quantity required
                    </div>
                </td>
                <td>
                    <div className="input-group">
                        <div className="input-group-prepend">
                            <span className="input-group-text">
                                <i className="fas fa-dollar-sign" />
                            </span>
                        </div>
                        <CurrencyControl
                            value={amount}
                            onChange={(v) => { this.updateItemProperty(id, 'amount', v) }}
                        />
                        <div className="invalid-feedback text-center ml-4">
                            Price required
                        </div>
                    </div>
                </td>
                <td>
                    ${ (quantity * amount).toFixed(2) }
                </td>
                <td>
                    <button className="btn-link btn-invoice-delete" onClick={() => this.removeItem(id)}>
                        <i className="fas fa-times" />
                    </button>
                </td>
            </tr>
        );
    }

    private createNewItem = () => {
        const items = this.state.items;

        const id = uuidv4();

        const newItems = {
            byHash: {
                ...items.byHash,
                [id]: {
                    amount: 0,
                    description: '',
                    id,
                    quantity: 0,
                },
            },
            byId: [...items.byId, id],
        };

        this.onItemsChange(newItems);
    }

    private removeItem = (id: string) => {
        const items = this.state.items;
        const newHash = {...items.byHash};
        delete newHash[id];

        const newItems = {
            byHash: newHash,
            byId: items.byId.filter(i => i !== id),
        };

        this.onItemsChange(newItems);

        // if this would set the list empty, add an empty one back
        if (newItems.byId.length < 1) {
            this.createNewItem();
        }
    }

    private updateItem = (id: string, item: InvoiceItem) => {
        const items = this.state.items;
        const newHash = {...items.byHash};
        newHash[id] = item;

        const newItems = {
            byHash: newHash,
            byId: items.byId,
        };

        this.onItemsChange(newItems);
    }

    private updateItemProperty = (id: string, name: string, value) => {
        const item = this.state.items.byHash[id];
        item[name] = value;
        this.updateItem(id, item);
    }

    private onItemsChange = (newItems: IState["items"]) => {
        // this.setState({ item: newItems });

        const itemArray = newItems.byId.map(i => newItems.byHash[i]);
        this.props.onItemsChange(itemArray);
    }

    private onDiscountChange = (value: InvoiceDiscount) => {
        // add calculation closure
        value.getCalculatedDiscount = this.calculateDiscount;
        this.props.onDiscountChange(value);
    }

    private onTaxPercentChange = (value: string) => {
        const tax = Number(value);
        // pass up the actual rate
        this.props.onTaxPercentChange(tax / 100);
    }

    private calculateSubTotal = () => {
        const items = this.state.items;
        const sum = items.byId.reduce((prev, id) => {
            const item = items.byHash[id];
            return prev + (item.quantity * item.amount);
        }, 0);

        return sum;
    }

    private calculateDiscount = () => {
        const { coupons, discount } = this.props;

        if (!discount.hasDiscount) {
            return 0;
        }

        if (discount.couponId) {
            // get selected coupon
            const selectedCoupon = coupons.find(c => c.id === discount.couponId);
            if (!selectedCoupon) {
                return 0;
            }
            
            if (!!selectedCoupon.expiresAt && isAfter(new Date(), selectedCoupon.expiresAt)) {
                return 0;
            }

            const { discountAmount, discountPercent } = selectedCoupon;

            if (discountAmount) {
                return discountAmount;
            }
            
            const sub = this.calculateSubTotal();
            return sub * discountPercent;
        }

        return discount.maunalAmount;
    }

    private calculateTaxAmount = () => {
        const { taxPercent } = this.props;
        const discount = this.calculateDiscount();
        const sub = this.calculateSubTotal();
        return (sub - discount) * taxPercent;
    }

    private calculateTotal = () => {
        const discount = this.calculateDiscount();
        const sub = this.calculateSubTotal();
        const tax = this.calculateTaxAmount();
        return sub - discount + tax;
    }
}