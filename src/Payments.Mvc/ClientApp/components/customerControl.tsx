import * as React from 'react';
import { InvoiceCustomer } from '../models/InvoiceCustomer';

const emailRegex = /^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$/;

interface IProps {
    customer: InvoiceCustomer;
    onChange: (value: InvoiceCustomer) => void;
}

export default class CustomerControl extends React.Component<IProps, {}> {

    public render() {
        const { name, email, address } = this.props.customer;

        return (
            <div>
                <div className="form-group">
                    <label>Customer Name</label>
                    <div className="input-group">
                        <div className="input-group-prepend">
                            <span className="input-group-text">
                                <i className="far fa-fw fa-user mr" />
                            </span>
                        </div>
                        <input
                            type="text"
                            className="form-control"
                            placeholder="Joe Person"
                            onChange={(e) => { this.onChange({ name: e.target.value }) }}
                            value={name}
                        />
                    </div>
                </div>
                <div className="form-group">
                    <label>Customer Email</label>
                    <div className="input-group">
                        <div className="input-group-prepend">
                            <span className="input-group-text">
                                <i className="far fa-fw fa-envelope mr" />
                            </span>
                        </div>
                        <input
                            type="email"
                            className="form-control"
                            placeholder="person@example.com"
                            onChange={(e) => { this.onChange({ email: e.target.value }) }}
                            value={email}
                            required={true}
                        />
                        <div className="invalid-feedback">
                            Email required.
                        </div>
                    </div>
                </div>
                <div className="form-group">
                    <label>Customer Address</label>
                    <div className="input-group">
                        <div className="input-group-prepend">
                            <span className="input-group-text">
                                <i className="far fa-fw fa-address-book mr" />
                            </span>
                        </div>
                        <input
                            type="text"
                            className="form-control"
                            placeholder="One Shields Ave, Davis, CA"
                            onChange={(e) => { this.onChange({ address: e.target.value }) }}
                            value={address}
                        />
                    </div>
                </div>
            </div>
        );
    }

    private onChange = (value: Partial<InvoiceCustomer>) => {
        const newCustomer = {
            ...this.props.customer,
            ...value,
        };
        this.props.onChange(newCustomer);
    }
}