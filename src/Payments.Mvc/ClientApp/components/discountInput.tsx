import * as React from 'react';

import { Coupon } from '../models/Coupon';
import { InvoiceDiscount } from '../models/InvoiceDiscount';

import CouponSelectControl from './couponSelectControl';
 
interface IProps {
    coupons: Coupon[];
    discount: InvoiceDiscount;
    onChange: (value: InvoiceDiscount) => void;
}

interface IState {
    isModalOpen: boolean
}

export default class DiscountInput extends React.PureComponent<IProps, IState> {

    constructor(props: IProps) {
        super(props);

        this.state = {
            isModalOpen: false,
        };
    }

    public render() {
        const { coupons, discount } = this.props;
        const { isModalOpen } = this.state;

        return (
            <div>
                { this.renderControl() }
                <CouponSelectControl
                    isModalOpen={isModalOpen}
                    onClose={this.closeModal}
                    onChange={this.onChange}
                    coupons={coupons}
                    value={discount}
                />
            </div>

        )
    }

    private renderControl() {
        const { discount } = this.props;

        if (!discount.hasDiscount) {
            return (
                <button className="btn btn-link" onClick={this.openModal}>
                    <i className="fas fa-plus mr-2" /> Add coupon
                </button>
            );
        }
        
        return (
            <div className="input-group">
                <div className="input-group-prepend">
                    <span className="input-group-text">
                        <i className="fas fa-dollar-sign" />
                    </span>
                </div>
                <input className="form-control text-right" value={discount.maunalAmount.toFixed(2)} readOnly={true} />
                <div className="invalid-feedback">
                    Set a discount or remove.
                </div>
            </div>
        );
    }

    private openModal = () => {
        this.setState({ isModalOpen: true });
    }

    private closeModal = () => {
        this.setState({ isModalOpen: false });
    }

    private onChange = (value: InvoiceDiscount) => {
        this.props.onChange(value);
    }
}