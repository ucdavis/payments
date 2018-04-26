import * as React from 'react';
import { InvoiceCustomer } from '../models/InvoiceCustomer';
 
const emailRegex = /^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$/;

interface IProps {
    customers: InvoiceCustomer[];
    onChange: (value: InvoiceCustomer[]) => void;
}

interface IState {
    hasMultipleCustomers: boolean;
    multiCustomerInput: string;
}

export default class DiscountInput extends React.Component<IProps, IState> {

    constructor(props) {
        super(props);
        const { customers } = props;

        this.state = {
            hasMultipleCustomers: customers && customers.length,
            multiCustomerInput: "",
        }
    }

    public render() {
        const { customers, onChange } = this.props;
        const { hasMultipleCustomers, multiCustomerInput } = this.state;

        if (!hasMultipleCustomers) {
            const customer = customers.length ? customers[0] : {};
            return (
                <div className="">
                    <h2>Customer Info</h2>
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
                                <button className="btn btn-outline-secondary" type="button" onClick={this.enableMultiCustomer}>
                                    <i className="fa fa-plus" />
                                    <i className="fa fa-users" />
                                    Bill Multiple Customers
                                </button>
                            </div>
                        </div>
                    </div>
                </div>
            );
        }

        return (
            <div className="">
                <h2>Customer Info</h2>
                <div className="form-group">
                    <label>Customer Emails</label>
                    <div className="">
                        {customers.map(c => this.renderCustomerTag(c))}
                    </div>
                    <div className="input-group">
                        <textarea
                            rows={4}
                            className="form-control"
                            placeholder="user1@example.com; user2@example.com"
                            onBlur={(e) => { this.updateCustomerList(e.target.value) }}
                            onChange={(e) => { this.updateProperty("multiCustomerInput", e.target.value) }}
                            value={multiCustomerInput}
                        />
                        <div className="input-group-append">
                            <button className="btn btn-outline-secondary" type="button" onClick={this.disableMultiCustomer}>
                                <i className="fa fa-user" />
                                Bill Single Customer
                            </button>
                        </div>
                    </div>
                </div>
            </div>
        );
    }

    private renderCustomerTag(customer: InvoiceCustomer) {
        return (
            <span className="badge badge-primary" key={customer.email}>
                {customer.email}
                <button className="btn btn-link" onClick={() => this.removeCustomer(customer.email)}><i className="fa fa-times" /></button>
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

    // TODO: manage duplicates
    private updateCustomerList = (value: string) => {
        const { customers, onChange } = this.props;

        const emails = value.split(/[,;\r\n]+/).filter(e => e.length);
        const validCustomers = [...customers];
        const invalidEmails = [];

        emails.forEach(e => {
            if (emailRegex.test(e)) {
                validCustomers.push({ email: e });
            } else {
                invalidEmails.push(e);
            }
        });

        onChange(validCustomers);
        this.setState({
            multiCustomerInput: invalidEmails.join("\n"),
        });
    }

    private removeCustomer = (email: string) => {
        const { customers, onChange } = this.props;
        onChange(customers.filter(c => c.email !== email));
    }
}