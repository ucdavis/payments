import * as React from 'react';
import { InvoiceCustomer } from '../models/InvoiceCustomer';

interface IProps {
  customer: InvoiceCustomer;
  onChange: (value: InvoiceCustomer) => void;
}

export default class CustomerControl extends React.Component<IProps, {}> {
  public render() {
    const { name, company, email, address } = this.props.customer;

    return (
      <div>
        <div className='form-group'>
          <label>Customer Name</label>
          <div className='input-group'>
            <div className='input-group-text'>
              <i className='far fa-fw fa-user mr' />
            </div>
            <input
              type='text'
              className='form-control'
              placeholder='Joe Person'
              onChange={e => {
                this.onChange({ name: e.target.value });
              }}
              value={name}
            />
          </div>
        </div>
        <div className='form-group'>
          <label>Customer Company</label>
          <div className='input-group'>
            <div className='input-group-text'>
              <i className='far fa-fw fa-building mr' />
            </div>
            <input
              type='text'
              className='form-control'
              placeholder='Company'
              onChange={e => {
                this.onChange({ company: e.target.value });
              }}
              value={company || ''}
            />
          </div>
        </div>
        <div className='form-group'>
          <label>Customer Email</label>
          <div className='input-group'>
            <div className='input-group-text'>
              <i className='far fa-fw fa-envelope mr' />
            </div>
            <input
              type='email'
              className='form-control'
              placeholder='person@example.com'
              onChange={e => {
                this.onChange({ email: e.target.value });
              }}
              value={email}
              required={true}
            />
            <div className='invalid-feedback text-end'>Email required</div>
          </div>
        </div>
        <div className='form-group'>
          <label>Customer Address</label>
          <div className='input-group'>
            <div className='input-group-text'>
              <i className='far fa-fw fa-address-book mr' />
            </div>
            <input
              type='text'
              className='form-control'
              placeholder='One Shields Ave, Davis, CA'
              onChange={e => {
                this.onChange({ address: e.target.value });
              }}
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
      ...value
    };
    this.props.onChange(newCustomer);
  };
}
