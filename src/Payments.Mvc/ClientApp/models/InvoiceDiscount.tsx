export interface InvoiceDiscount {
    hasDiscount: boolean;

    couponId?: number;
    maunalAmount?: number;

    getCalculatedDiscount?: () => number;
}