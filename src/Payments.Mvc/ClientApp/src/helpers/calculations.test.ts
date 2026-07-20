import {
  calculatePercentageAmount,
  calculateTotal,
  roundCurrency
} from './calculations';

describe('currency calculations', () => {
  it.each([
    [9.349, 9.35],
    [9.344, 9.34],
    [9.345, 9.35]
  ])('rounds %s to %s', (value, expected) => {
    expect(roundCurrency(value)).toBe(expected);
  });

  it('rounds the final invoice total to currency precision', () => {
    const items = [
      {
        id: 1,
        description: 'Recharge item',
        quantity: 0.25,
        amount: 10.02,
        taxExempt: false,
        total: 0
      }
    ];

    expect(calculateTotal(items, { hasDiscount: false }, 0)).toBe(2.51);
  });

  it('uses the rounded invoice total for a 100 percent allocation', () => {
    const invoiceTotal = calculateTotal(
      [
        {
          id: 1,
          description: 'Recharge item',
          quantity: 0.25,
          amount: 10.02,
          taxExempt: false,
          total: 0
        }
      ],
      { hasDiscount: false },
      0
    );

    expect(calculatePercentageAmount(invoiceTotal, 100)).toBe(2.51);
  });
});
