import * as React from 'react';
import { InvoiceCustomer } from '../models/InvoiceCustomer';

const emailRegex = /^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$/;

interface IProps {
    customer: InvoiceCustomer;
    onChange: (value: InvoiceCustomer) => void;
}

export default class CustomerControl extends React.Component<IProps, {}> {

    public render() {
        const { customer, onChange } = this.props;

        return (
            <input
                type="email"
                className="form-control"
                placeholder="johndoe@example.com"
                onChange={(e) => { onChange({ email: e.target.value }) }}
                value={customer.email}
                required={true}
            />
        );
    }
}