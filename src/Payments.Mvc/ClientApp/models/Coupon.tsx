export interface Coupon {
    id: number;
    name: string;
    code: string;

    discountAmount?: number;
    discountPercentage?: number;

    expiresAt?: Date;
}