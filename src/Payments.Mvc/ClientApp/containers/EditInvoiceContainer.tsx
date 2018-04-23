import * as React from 'react';
import { Invoice } from '../models/Invoice';

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

    public renderItem(item) {
        return (
            <tr>
                <td></td>
                <td></td>
                <td></td>
                <td></td>
            </tr>
        );
    }
}
