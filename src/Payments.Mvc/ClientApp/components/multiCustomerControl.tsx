import * as React from 'react';
import { InvoiceCustomer } from '../models/InvoiceCustomer';
import * as ArrayUtils from '../utils/array.js'; 

const emailRegex = /^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$/;

interface IProps {
    customers: InvoiceCustomer[];
    onChange: (value: InvoiceCustomer[]) => void;
}

interface IState {
    hasMultipleCustomers: boolean;
    multiCustomerInput: string;
}

export default class MultiCustomerControl extends React.Component<IProps, IState> {

    constructor(props) {
        super(props);
        const { customers } = props;

        this.state = {
            hasMultipleCustomers: customers && customers.length,
            multiCustomerInput: "",
        }
    }

    public render() {
        return (
            <div className="multi-customer-control">
                <h2>Customer Info</h2>
                { this.renderContent() }
            </div>
        );
    }

    private renderContent() {
        const { customers, onChange } = this.props;
        const { hasMultipleCustomers, multiCustomerInput } = this.state;

        if (!hasMultipleCustomers) {
            const customer = customers.length ? customers[0] : {};
            return (
                <div className="form-group">
                    <label>Customer Email</label>
                    <div className="input-group">
                        <input
                            type="email"
                            className="form-control"
                            placeholder="johndoe@example.com"
                            onChange={(e) => { onChange([{ email: e.target.value }]) }}
                            value={customer.email}
                        />
                        <div className="input-group-append">
                            <button className="btn" type="button" onClick={this.enableMultiCustomer}>
                                <i className="fa fa-plus mr-3" />
                                <i className="fa fa-users mr-2" />
                                Bill Multiple Customers
                            </button>
                        </div>
                    </div>
                </div>
            );
        }

        return (
            <div className="form-group">
                <label>Customer Emails</label>
                <div className="email-badge-row">
                    {customers.map(c => this.renderCustomerTag(c))}
                </div>
                <textarea
                    rows={4}
                    className="form-control mb-2"
                    placeholder="user1@example.com; user2@example.com"
                    onBlur={(e) => { this.updateCustomerList(e.target.value) }}
                    onChange={(e) => { this.updateProperty("multiCustomerInput", e.target.value) }}
                    value={multiCustomerInput}
                />
                <div className="row justify-content-end">
                    <button className="btn" type="button" onClick={this.disableMultiCustomer}>
                        <i className="fa fa-user mr-2" />
                        Bill Single Customer
                    </button>
                </div>
            </div>
        );
    }

    private renderCustomerTag(customer: InvoiceCustomer) {
        return (
            <span className="badge badge-primary" key={customer.email}>
                {customer.email}
                <button className="btn-plain" onClick={() => this.removeCustomer(customer.email)}><i className="fa fa-times" /></button>
            </span>
        );
    }

    private enableMultiCustomer = () => {
        this.setState({
            hasMultipleCustomers: true,
        })
    }

    private disableMultiCustomer = () => {
        this.setState({
            hasMultipleCustomers: false,
        })
    }

    private updateProperty = (name: any, value: any) => {
        this.setState({
            [name]: value
        });
    }

    private updateCustomerList = (value: string) => {
        const { customers, onChange } = this.props;

        const emails = value
            .split(/[,;\r\n]+/)
            .filter(e => e.length)
            .map(e => e.trim());

        const validCustomers = [...customers];
        const invalidEmails = [];

        emails.forEach(e => {
            if (emailRegex.test(e)) {
                validCustomers.push({ email: e });
            } else {
                invalidEmails.push(e);
            }
        });

        // remove duplicates, map to customer
        const distinctCustomers = ArrayUtils.distinct(validCustomers, c => c.email);
        const sortedCustomers = distinctCustomers.sort((c1, c2) => c1.email > c2.email);

        onChange(sortedCustomers);
        this.setState({
            multiCustomerInput: invalidEmails.join("\n"),
        });
    }

    private removeCustomer = (email: string) => {
        const { customers, onChange } = this.props;
        onChange(customers.filter(c => c.email !== email));
    }
}