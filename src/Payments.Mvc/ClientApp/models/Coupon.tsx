export interface Coupon {
    id: number;
    name: string;
    code: string;

    discountAmount?: number;
    discountPercent?: number;

    expiresAt?: Date;
}