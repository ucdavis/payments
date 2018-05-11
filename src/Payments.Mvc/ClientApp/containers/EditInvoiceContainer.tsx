import "isomorphic-fetch";
import * as React from 'react';
import { Invoice } from '../models/Invoice';
import { InvoiceCustomer } from '../models/InvoiceCustomer';
import { InvoiceItem } from '../models/InvoiceItem';

import DiscountInput from '../components/discountInput';
import MemoInput from '../components/memoInput';
import TaxInput from '../components/taxInput';

declare var antiForgeryToken: string;

const emailRegex = /^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$/;

interface IProps {
    invoice: Invoice;
}

interface IState {
    customer: InvoiceCustomer;
    discount: number;
    taxRate: number;
    memo: string;
    items: {
        byId: number[];
        byHash: {
            [key: number]: InvoiceItem;
        };
    };
}

export default class EditInvoiceContainer extends React.Component<IProps, IState> {
    constructor(props: IProps) {
        super(props);

        const { invoice } = this.props;

        // map array to object
        const items: IState["items"] = {
            byHash: {},
            byId: [],
        };

        // require at least one item
        if (!invoice.items || invoice.items.length < 1) {
            invoice.items = [{
                amount: 0,
                description: '',
                id: 0,
                quantity: 0,
            }];
        }

        props.invoice.items.forEach((item, index) => {
            const id = item.id;
            items.byId.push(id);
            items.byHash[id] = item;
        });

        this.state = {
            customer: {
                address: invoice.customerAddress || "",
                email: invoice.customerEmail || "",
                name: invoice.customerName || "",
            },
            discount: invoice.discount || 0,
            items,
            memo: invoice.memo,
            taxRate: invoice.taxPercent || 0,
        };
    }

    public render() {
        const { customer, items, discount, taxRate, memo } = this.state;
        const subtotal = this.calculateSubTotal();
        const tax = this.calculateTaxAmount();
        const total = this.calculateTotal();

        return (
            <div className="container">
                <h1>Edit Invoice</h1>
                <h2>Customer Info</h2>
                <div className="form-group">
                    <label>Customer Email</label>
                    <input
                        type="email"
                        className="form-control"
                        placeholder="johndoe@example.com"
                        onChange={(e) => { this.updateProperty("customer", { email: e.target.value }) }}
                        value={customer.email}
                    />
                </div>
                <div className="">
                    <h2>Invoice Items</h2>
                    <table className="table">
                        <thead>
                            <tr>
                                <th>Description</th>
                                <th>Qty</th>
                                <th>Price</th>
                                <th>Amount</th>
                                <th />
                            </tr>
                        </thead>
                        <tbody>
                            { items.byId.map((id) => this.renderItem(id, items.byHash[id])) }
                        </tbody>
                        <tbody>
                            <tr>
                                <td>
                                    <button className="btn btn-link" onClick={this.createNewItem}>
                                        <i className="fa fa-plus" /> Add another item
                                    </button>
                                </td>
                                <td>Subtotal</td>
                                <td />
                                <td>${ subtotal.toFixed(2) }</td>
                                <td />
                            </tr>
                            <tr>
                                <td />
                                <td>Discount</td>
                                <td><DiscountInput value={discount} onChange={(v) => this.updateProperty('discount', v)} /></td>
                                <td>$({ (Number(discount)).toFixed(2) })</td>
                                <td />
                            </tr>
                            <tr>
                                <td />
                                <td>Tax</td>
                                <td><TaxInput value={taxRate} onChange={(v) => this.updateProperty('taxRate', v)} /></td>
                                <td>${ tax.toFixed(2) }</td>
                                <td />
                            </tr>
                        </tbody>
                        <tfoot>
                            <tr>
                                <td />
                                <td>Total</td>
                                <td />
                                <td>${ total.toFixed(2) }</td>
                                <td />
                            </tr>
                        </tfoot>
                    </table>
                </div>
                <div className="">
                    <h2>Memo</h2>
                    <div className="form-group">
                        <MemoInput value={memo} onChange={(v) => this.updateProperty('memo', v)} />
                    </div>
                </div>
                <div className="">
                    <h2>Billing</h2>
                </div>
                <div className="">
                    <button className="btn btn-default">Cancel</button>
                    <button className="btn btn-success" onClick={this.onSend}>Send</button>
                    <button className="btn btn-success" onClick={this.onSubmit}>Save</button>
                </div>
            </div>
        );
    }

    private renderItem(id: number, item: InvoiceItem) {
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
                    />
                </td>
                <td>
                    <input
                        type="number"
                        min="0"
                        className="form-control"
                        placeholder="0"
                        value={quantity}
                        onChange={(e) => { this.updateItemProperty(id, 'quantity', e.target.value) }}
                    />
                </td>
                <td>
                    <div className="input-group">
                        <div className="input-group-prepend">
                            <span className="input-group-text">$</span>
                        </div>
                        <input
                            type="number"
                            min="0"
                            step="0.01"
                            className="form-control"
                            placeholder="0.00"
                            value={amount}
                            onChange={(e) => { this.updateItemProperty(id, 'amount', e.target.value) }}
                        />
                    </div>
                </td>
                <td>
                    ${ (quantity * amount).toFixed(2) }
                </td>
                <td>
                    <button className="btn btn-link" onClick={() => this.removeItem(id)}>
                        <i className="fa fa-times" />
                    </button>
                </td>
            </tr>
        );
    }

    private updateProperty = (name: any, value: any) => {
        this.setState({
            [name]: value
        });
    }

    private createNewItem = () => {
        const items = this.state.items;
        // needs new id logic
        const id = items.byId.reduce((max, value) => Math.max(max, value), 0) + 1;
        this.setState({
            items: {
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
            },
        });
    }

    private removeItem = (id) => {
        const items = this.state.items;
        const newHash = {...items.byHash};
        delete newHash[id];

        const newItems = {
            byHash: newHash,
            byId: items.byId.filter(i => i !== id),
        };

        this.setState({
            items: newItems,
        });

        // if this would set the list empty, add an empty one back
        if (newItems.byId.length < 1) {
            this.createNewItem();
        }
    }

    private updateItem = (id, item) => {
        const items = this.state.items;
        const newHash = {...items.byHash};
        newHash[id] = item;

        this.setState({
            items: {
                byHash: newHash,
                byId: items.byId,
            },
        });
    }

    private updateItemProperty = (id, name, value) => {
        const item = this.state.items.byHash[id];
        item[name] = value;
        this.updateItem(id, item);
    }

    private calculateSubTotal = () => {
        const items = this.state.items;
        const sum = items.byId.reduce((prev, id) => {
            const item = items.byHash[id];
            return prev + (item.quantity * item.amount);
        }, 0);

        return sum;
    }

    private calculateTaxAmount = () => {
        const { discount, taxRate } = this.state;
        const sub = this.calculateSubTotal();
        return (sub - discount) * taxRate;
    }

    private calculateTotal = () => {
        const { discount } = this.state;
        const sub = this.calculateSubTotal();
        const tax = this.calculateTaxAmount();
        return sub - discount + tax;
    }

    private onSubmit = async () => {
        const { id } = this.props.invoice;
        const { customer, discount, taxRate, items, memo } = this.state;

        const invoiceItems = items.byId.map(itemId => items.byHash[itemId]);

        // create submit object
        const invoice = {
            customer,
            discount,
            items: invoiceItems,
            memo,
            tax: taxRate,
        };

        // create save url
        const url = `/invoices/edit/${id}`;

        // fetch 
        const response = await fetch(url, {
            body: JSON.stringify(invoice),
            credentials: "same-origin",
            headers: new Headers({
                "Content-Type": "application/json",
                "RequestVerificationToken": antiForgeryToken
            }),
            method: "POST",
        });
        console.log(await response.json());

        // redirect to invoices
    }

    private onSend = async () => {
        const { id } = this.props.invoice;

        const url = `/invoices/send/${id}`;

        const response = await fetch(url, {
            credentials: "same-origin",
            headers: new Headers({
                "Content-Type": "application/json",
                "RequestVerificationToken": antiForgeryToken
            }),
            method: "POST",
        });
        console.log(await response.json());
    }
}
