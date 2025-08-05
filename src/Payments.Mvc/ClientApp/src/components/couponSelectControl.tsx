import * as React from 'react';

import { isAfter } from 'date-fns';

import { Coupon } from '../models/Coupon';
import { InvoiceDiscount } from '../models/InvoiceDiscount';

import CurrencyControl from './currencyControl';
import Modal from './modal';

interface IProps {
  coupons: Coupon[];
  value: InvoiceDiscount;
  isModalOpen: boolean;
  onChange: (value: InvoiceDiscount) => void;
  onClose: () => void;
}

interface IState {
  manualAmount: number;
}

function sortCoupons(a, b) {
  if (a.isDefault) {
    return -1;
  }

  if (b.isDefault) {
    return 1;
  }

  if (a.name < b.name) {
    return -1;
  }

  if (a.name > b.name) {
    return 1;
  }

  return 0;
}

export default class CouponSelectControl extends React.Component<
  IProps,
  IState
> {
  constructor(props) {
    super(props);

    this.state = {
      manualAmount: 0
    };
  }

  public render() {
    const { coupons, isModalOpen } = this.props;
    const { manualAmount } = this.state;

    const ordered = [...coupons].sort(sortCoupons);

    return (
      <Modal
        dialogClassName='account-details-modal modal-lg'
        isOpen={isModalOpen}
        onBackdropClick={this.closeModal}
        onEscape={this.closeModal}
      >
        <div className='modal-header'>
          <div className='row flex-grow-1'>
            <div className='col-md-3' />
            <div className='col-md-6 d-flex justify-content-center align-items-center'>
              <span className='modal-title'>Coupons Details</span>
            </div>
            <div className='col-md-3 d-flex justify-content-end align-items-center'>
              <button
                type='button'
                className='close m-1'
                onClick={this.closeModal}
              >
                <span aria-hidden='true'>
                  <i className='fas fa-times' />
                </span>
              </button>
            </div>
          </div>
        </div>
        {ordered.map(this.renderCouponDetails)}
        <div className='modal-body'>
          <label>Or manually set a discount:</label>
          <div className='input-group'>
            <div className='input-group-prepend'>
              <span className='input-group-text'>
                <i className='fas fa-dollar-sign' />
              </span>
            </div>
            <CurrencyControl
              value={manualAmount}
              onChange={this.onAmountChange}
            />
            <div className='input-group-append'>
              <button
                className='btn'
                type='button'
                onClick={this.modalSetAmount}
              >
                Set Discount Amount <i className='fas fa-fw fa-check ml-3' />
              </button>
            </div>
          </div>
        </div>
      </Modal>
    );
  }

  private renderCouponDetails = (coupon: Coupon) => {
    const discount = this.props.value;
    const {
      id,
      name,
      code,
      discountPercent,
      discountAmount,
      expiresAt
    } = coupon;

    let isExpired = false;
    if (expiresAt) {
      isExpired = isAfter(new Date(), expiresAt);
    }

    return (
      <div className='modal-body' key={id}>
        <div className='row flex-grow-1'>
          <div className='col-md-4'>
            <span className='account-name'>{name}</span>
            <p className='account-description'>{code}</p>
          </div>
          <div className='col-md-4 d-flex flex-column'>
            <dl className='row'>
              <dt className='col-sm-4'>Percentage</dt>
              <dd className='col-sm-8'>{discountPercent * 100}%</dd>

              <dt className='col-sm-4'>Amount</dt>
              <dd className='col-sm-8'>${discountAmount}</dd>

              <dt className='col-sm-4'>Expires On</dt>
              <dd className='col-sm-8'>{expiresAt}</dd>
            </dl>
          </div>
          <div className='col-md-3 d-flex flex-column justify-content-around align-items-center'>
            {isExpired && <span className='badge text-bg-info'>expired</span>}
            {discount.couponId === id && (
              <span className='badge text-bg-success'>selected</span>
            )}
          </div>
          <div className='col-md-1 d-flex justify-content-end align-items-center'>
            <button
              className='btn'
              onClick={() => this.modalPickCoupon(coupon)}
            >
              <i className='fas fa-angle-right' />
            </button>
          </div>
        </div>
      </div>
    );
  };

  private onAmountChange = (value: number) => {
    this.setState({
      manualAmount: value
    });
  };

  private closeModal = () => {
    this.props.onClose();
  };

  private modalPickCoupon = (coupon: Coupon) => {
    this.props.onChange({
      coupon,
      couponId: coupon.id,
      hasDiscount: true,
      maunalAmount: null
    });
    this.closeModal();
  };

  private modalSetAmount = () => {
    this.props.onChange({
      coupon: null,
      couponId: null,
      hasDiscount: true,
      maunalAmount: this.state.manualAmount
    });
    this.closeModal();
  };
}
