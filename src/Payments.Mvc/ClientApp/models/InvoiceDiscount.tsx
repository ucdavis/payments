import { Coupon } from "./Coupon";

export interface InvoiceDiscount {
    hasDiscount: boolean;

    couponId?: number;
    coupon?: Coupon;

    maunalAmount?: number;

    getCalculatedDiscount?: () => number;
}