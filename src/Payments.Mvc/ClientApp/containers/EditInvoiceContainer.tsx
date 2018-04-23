import * as React from 'react';
import { Invoice } from '../models/Invoice';
import { InvoiceItem } from '../models/InvoiceItem';

interface IProps {
    invoice: Invoice;
}

interface IState {
    title: string;
}

export default class EditInvoiceContainer extends React.Component<IProps, IState> {
    

    public render() {
        const { invoice } = this.props;

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
                            </tr>
                        </thead>
                        <tbody>
                            { invoice.items.map(this.renderItem) }
                        </tbody>
                        
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

    public renderItem(item: InvoiceItem) {
        const { description, quantity, price } = item;

        return (
            <tr>
                <td>
                    <input
                        type="text"
                        className="form-control"
                        placeholder=""
                    />
                </td>
                <td>
                <input
                        type="text"
                        className="form-control"
                        placeholder="0"
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
                        />
                    </div>
                </td>
                <td>
                    ${ (quantity * price).toFixed(2) }
                </td>
            </tr>
        );
    }

    private updateProperty = () => {
    }

    private updateItemProperty = () => {
    }
}
