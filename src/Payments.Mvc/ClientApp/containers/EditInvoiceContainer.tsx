import * as React from 'react';
import { Invoice } from '../models/Invoice';
import { InvoiceItem } from '../models/InvoiceItem';

interface IProps {
    invoice: Invoice;
}

interface IState {
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

        // map array to object
        const items: IState["items"] = {
            byHash: {},
            byId: [],
        };
        props.invoice.items.forEach((item, index) => {
            const id = item.id;
            items.byId.push(id);
            items.byHash[id] = item;
        });

        this.state = {
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
                    Scott Kirkland
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
                    <button className="btn btn-success">Save</button>
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

    private updateProperty = (name, value) => {
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

        this.setState({
            items: {
                byHash: newHash,
                byId: items.byId.filter(i => i !== id),
            },
        });
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
}
