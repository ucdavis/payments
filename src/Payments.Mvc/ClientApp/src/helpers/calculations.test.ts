import {
  calculatePercentageAmount,
  calculateSubTotal,
  calculateTaxAmount,
  calculateTotal,
  roundCurrency
} from './calculations';

describe('currency calculations', () => {
  it.each([
    [9.349, 9.35],
    [9.344, 9.34],
    [9.345, 9.35],
    [-9.345, -9.35]
  ])('rounds %s to %s', (value, expected) => {
    expect(roundCurrency(value)).toBe(expected);
  });

  it.each([
    [0.06, 155.5, 9.33],
    [0.06, 155.83, 9.35],
    [0.06, 155.74, 9.34],
    [0.25, 10.02, 2.51],
    [0.01, 10.5, 0.11]
  ])(
    'rounds quantity %s at unit price %s to invoice total %s',
    (quantity, amount, expected) => {
      const items = [
        {
          id: 1,
          description: 'Recharge item',
          quantity,
          amount,
          taxExempt: false,
          total: 0
        }
      ];

      expect(calculateTotal(items, { hasDiscount: false }, 0)).toBe(expected);
    }
  );

  it('rounds each line item before summing', () => {
    const items = [
      {
        id: 1,
        description: 'First item',
        quantity: 0.5,
        amount: 0.01,
        taxExempt: false,
        total: 0
      },
      {
        id: 2,
        description: 'Second item',
        quantity: 0.5,
        amount: 0.01,
        taxExempt: false,
        total: 0
      }
    ];

    expect(calculateSubTotal(items)).toBe(0.02);
    expect(calculateTotal(items, { hasDiscount: false }, 0)).toBe(0.02);
  });

  it('rounds tax before calculating the invoice total', () => {
    const items = [1, 2, 3].map(id => ({
      id,
      description: 'Taxable item',
      quantity: 0.06,
      amount: 155.74,
      taxExempt: false,
      total: 9.34
    }));
    const discount = { hasDiscount: false };

    expect(calculateTaxAmount(items, discount, 0.07)).toBe(1.96);
    expect(calculateTotal(items, discount, 0.07)).toBe(29.98);
  });

  it('returns zero tax when the subtotal is zero', () => {
    const items = [
      {
        id: 1,
        description: 'Zero amount item',
        quantity: 0,
        amount: 10,
        taxExempt: false,
        total: 0
      }
    ];

    expect(calculateTaxAmount(items, { hasDiscount: false }, 0.07)).toBe(0);
  });

  it.each([
    [0.06, 155.83, 9.35],
    [0.06, 155.74, 9.34],
    [0.25, 10.02, 2.51],
    [0.01, 10.5, 0.11]
  ])(
    'uses rounded total %s x %s for a 100 percent recharge allocation',
    (quantity, amount, expected) => {
      const invoiceTotal = calculateTotal(
        [
          {
            id: 1,
            description: 'Recharge item',
            quantity,
            amount,
            taxExempt: false,
            total: 0
          }
        ],
        { hasDiscount: false },
        0
      );

      expect(calculatePercentageAmount(invoiceTotal, 100)).toBe(expected);
    }
  );
});
