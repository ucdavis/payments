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
                description: '',
                id: 0,
                price: 0,
                quantity: 0,
            }];
        }

        props.invoice.items.forEach((item, index) => {
            const id = item.id;
            items.byId.push(id);
            items.byHash[id] = item;
        });

        this.state = {
            customerAddress: "",
            customerEmail: "",
            customerName: "",
            items
        };
    }

    public render() {
        const { items } = this.state;

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
                        />
                    </div>
                    <div className="form-group">
                        <label>Customer Email</label>
                        <input
                            type="email"
                            className="form-control"
                            placeholder="johndoe@example.com"
                            onChange={(e) => { this.updateProperty("customerEmail", e.target.value) }}
                        />
                    </div>
                    <div className="form-group">
                        <label>Customer Adress</label>
                        <input
                            type="text"
                            className="form-control"
                            placeholder="123 Street, Davis, CA 95616"
                            onChange={(e) => { this.updateProperty("customerAddress", e.target.value) }}
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
                                <td>${ (0).toFixed(2) }</td>
                                <td />
                            </tr>
                            <tr>
                                <td />
                                <td>Discount</td>
                                <td>
                                    <button className="btn btn-link"><i className="fa fa-plus" /> Add coupon</button>
                                </td>
                                <td>${ (0).toFixed(2) }</td>
                                <td />
                            </tr>
                            <tr>
                                <td />
                                <td>Tax</td>
                                <td>
                                    <button className="btn btn-link"><i className="fa fa-plus" /> Add tax</button>
                                </td>
                                <td>${ (0).toFixed(2) }</td>
                                <td />
                            </tr>
                        </tbody>
                        <tfoot>
                            <tr>
                                <td />
                                <td>Total</td>
                                <td />
                                <td>${ this.calculateTotal().toFixed(2) }</td>
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

    private renderItem(id: number, item: InvoiceItem) {
        const { description, quantity, price } = item;

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
                        type="text"
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
                            type="text"
                            className="form-control"
                            placeholder="0.00"
                            value={price}
                            onChange={(e) => { this.updateItemProperty(id, 'price', e.target.value) }}
                        />
                    </div>
                </td>
                <td>
                    ${ (quantity * price).toFixed(2) }
                </td>
                <td>
                    <button className="btn btn-link" onClick={() => this.removeItem(id)}>
                        <i className="fa fa-times" />
                    </button>
                </td>
            </tr>
        );
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
                        description: '',
                        id,
                        price: 0,
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

    private calculateTotal = () => {
        const items = this.state.items;
        const sum = items.byId.reduce((prev, id) => {
            const item = items.byHash[id];
            return prev + (item.quantity * item.price);
        }, 0);

        return sum;
    }

    private onSubmit = async () => {
        const { id } = this.props.invoice;
        const { customerName, customerEmail, customerAddress, items } = this.state;

        const invoiceItems = items.byId.map(itemId => items.byHash[itemId]);

        // create submit object
        const invoice = {
            customerAddress,
            customerEmail,
            customerName,
            items: invoiceItems,
        };

        // set save url
        let url = "/invoices/create";
        if (id) {
            url = `/invoices/edit/${id}`;
        }

        // fetch 
        const response = await fetch(url, {
            // body: JSON.stringify({
            //     __RequestVerificationToken: antiForgeryToken,
            //     model: invoice,
            // }),
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