import { isAfter } from 'date-fns';

import { InvoiceDiscount } from "../models/InvoiceDiscount";
import { InvoiceItem } from "../models/InvoiceItem";

export function calculateSubTotal(items: InvoiceItem[]) {
    const sum = items.reduce((prev, item) => {
        return prev + (item.quantity * item.amount);
    }, 0);

    return sum;
}

export function calculateTaxableSubTotal(items: InvoiceItem[]) {
    const sum = items.reduce((prev, item) => {
        if (item.taxExempt) {
            return prev;
        }

        return prev + (item.quantity * item.amount);
    }, 0);

    return sum;
}

export function calculateDiscount(items: InvoiceItem[], discount: InvoiceDiscount) {
    if (!discount.hasDiscount) {
        return 0;
    }

    if (discount.couponId) {
        // get selected coupon
        const coupon = discount.coupon;
        if (!coupon) {
            return 0;
        }
        
        if (!!coupon.expiresAt && isAfter(new Date(), coupon.expiresAt)) {
            return 0;
        }

        const { discountAmount, discountPercent } = coupon;

        if (discountAmount) {
            return discountAmount;
        }
        
        const sub = calculateSubTotal(items);
        return sub * discountPercent;
    }

    return discount.maunalAmount;
}

export function calculateTaxAmount(items: InvoiceItem[], discount: InvoiceDiscount, taxPercent: number) {

    const sub = calculateSubTotal(items);
    const totalDiscount = calculateDiscount(items, discount);

    const taxableSub = calculateTaxableSubTotal(items);
    if (taxableSub <= 0) {
        return 0;
    }

    const taxableDiscount = totalDiscount * (taxableSub / sub);

    return (taxableSub - taxableDiscount) * taxPercent;
}

export function calculateTotal(items: InvoiceItem[], discount: InvoiceDiscount, taxPercent: number) {

    const totalDiscount = calculateDiscount(items, discount);
    const sub = calculateSubTotal(items);
    const tax = calculateTaxAmount(items, discount, taxPercent);
    return sub - totalDiscount + tax;
}
