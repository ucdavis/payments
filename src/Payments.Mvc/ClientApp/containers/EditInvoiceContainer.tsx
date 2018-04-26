import "isomorphic-fetch";
import * as React from 'react';
import { Invoice } from '../models/Invoice';
import { InvoiceItem } from '../models/InvoiceItem';

declare var antiForgeryToken: string;

interface IProps {
    invoice: Invoice;
}

interface IState {
    customerName: string;
    customerEmail: string;
    customerAddress: string;
    discount: number;
    hasDiscount: boolean;
    taxRate: number;
    hasTax: boolean;
    items: {
        byId: number[];
        byHash: {
            [key: number]: InvoiceItem;
        }
    };
}

export default class EditInvoiceContainer extends React.Component<IProps, IState> {
    constructor(props: IProps) {
        super(props);
        console.log("props:", props);
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
            customerAddress: invoice.customerAddress,
            customerEmail: invoice.customerEmail,
            customerName: invoice.customerName,
            discount: invoice.discount,
            hasDiscount: !!invoice.discount,
            hasTax: !!invoice.taxPercent,
            items,
            taxRate: invoice.taxPercent,
        };

        console.log(this.state);
    }

    public render() {
        const { items, discount, taxRate, customerAddress, customerEmail, customerName } = this.state;
        const subtotal = this.calculateSubTotal();
        const tax = this.calculateTaxAmount();
        const total = this.calculateTotal();

        return (
            <div className="container">
                <h1>Edit Invoice</h1>
                <div className="">
                    <h2>Customer Info</h2>
                    <div className="form-group">
                        <label>Customer Name</label>
                        <input
                            type="text"
                            className="form-control"
                            placeholder="John Doe"
                            onChange={(e) => { this.updateProperty("customerName", e.target.value) }}
                            value={customerName}
                        />
                    </div>
                    <div className="form-group">
                        <label>Customer Email</label>
                        <input
                            type="email"
                            className="form-control"
                            placeholder="johndoe@example.com"
                            onChange={(e) => { this.updateProperty("customerEmail", e.target.value) }}
                            value={customerEmail}
                        />
                    </div>
                    <div className="form-group">
                        <label>Customer Adress</label>
                        <input
                            type="text"
                            className="form-control"
                            placeholder="123 Street, Davis, CA 95616"
                            onChange={(e) => { this.updateProperty("customerAddress", e.target.value) }}
                            value={customerAddress}
                        />
                    </div>
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
                                <td>{ this.renderDiscountControl() }</td>
                                <td>$({ (Number(discount)).toFixed(2) })</td>
                                <td />
                            </tr>
                            <tr>
                                <td />
                                <td>Tax</td>
                                <td>{ this.renderTaxControl() }</td>
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
                        <textarea className="form-control" />
                    </div>
                </div>
                <div className="">
                    <h2>Billing</h2>
                </div>
                <div className="">
                    <button className="btn btn-default">Cancel</button>
                    <button className="btn btn-success" onClick={this.onSubmit}>Save</button>
                </div>
            </div>
        );
    }

    private renderDiscountControl() {
        if (!this.state.hasDiscount) {
            return (
                <button className="btn btn-link" onClick={this.addDiscount}>
                    <i className="fa fa-plus" /> Add coupon
                </button>
            );
        }
        
        return (
            <input
                type="number"
                min="0"
                step="0.01"
                className="form-control"
                placeholder="0.00"
                value={this.state.discount}
                onChange={(e) => { this.updateProperty('discount', e.target.value) }}
            />
        );
    }

    private renderTaxControl() {
        if (!this.state.hasTax) {
            return (
                <button className="btn btn-link" onClick={this.addTax}>
                    <i className="fa fa-plus" /> Add tax
                </button>
            );
        }

        return (
            <input
                type="number"
                min="0"
                step="0.01"
                className="form-control"
                placeholder="0.00"
                value={this.state.taxRate}
                onChange={(e) => { this.updateProperty('taxRate', e.target.value) }}
            />
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

    private addDiscount = () => {
        this.setState({ hasDiscount: true });
    }

    private addTax = () => {
        this.setState({ hasTax: true });
    }

    private updateProperty = (name: string, value: string) => {
        // https://github.com/Microsoft/TypeScript/issues/13948
        // computed key has to be cast as any
        this.setState({
            [name as any]: value
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
        const { customerName, customerEmail, customerAddress, discount, taxRate, items } = this.state;

        const invoiceItems = items.byId.map(itemId => items.byHash[itemId]);

        // create submit object
        const invoice = {
            customerAddress,
            customerEmail,
            customerName,
            discount,
            items: invoiceItems,
            tax: taxRate,
        };

        const url = `/invoices/save/${id}`;

        // fetch 
        const response = await fetch(url, {
            body: JSON.stringify(invoice),
            credentials: "include",
            headers: new Headers({
                "Content-Type": "application/json",
                "RequestVerificationToken": antiForgeryToken
            }),
            method: "POST"
        });
        console.log(await response.json());

        // redirect to invoices
    }
}
