import * as React from 'react';

import { isAfter } from 'date-fns';

import { Coupon } from '../models/Coupon';
import { InvoiceDiscount } from '../models/InvoiceDiscount';

import CouponSelectControl from './couponSelectControl';
import CurrencyControl from './currencyControl';
 
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
        const { coupons, discount } = this.props;

        if (!discount.hasDiscount) {
            return (
                <button className="btn btn-link" onClick={this.openModal}>
                    <i className="fas fa-plus mr-2" /> Add coupon
                </button>
            );
        }

        // not using a coupon
        if (!discount.couponId) {
            return (
                <div className="input-group">
                    <div className="input-group-prepend">
                        <span className="input-group-text">
                            <i className="fas fa-dollar-sign" />
                        </span>
                    </div>
                    <CurrencyControl
                        value={discount.maunalAmount}
                        onChange={this.onManualAmountChange}
                    />
                </div>
            );
        }

        // find coupon
        const coupon = coupons.find(c => c.id === discount.couponId);
        if (!coupon) {
            return null;
        }

        const expired = !!coupon.expiresAt && isAfter(new Date(), coupon.expiresAt)

        return (
            <div className="text-right">
                <strong>{coupon.name}</strong><br />
                {
                    (expired) &&
                    <span className="badge badge-info mr-3">Expired</span>
                }
                { 
                    (!!coupon.discountAmount) &&
                    <small>${coupon.discountAmount} off</small>
                }
                { 
                    (!!coupon.discountPercent) && 
                    <small>{coupon.discountPercent * 100}% off</small>
                }
            </div>
        )
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

    private onManualAmountChange = (value: number) => {
        this.onChange({
            coupon: null,
            couponId: null,
            hasDiscount: true,
            maunalAmount: value,
        })
    }
}