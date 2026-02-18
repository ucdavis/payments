/**
 * Formats a number as USD currency
 * @param amount - The amount to format
 * @returns Formatted currency string (e.g., "$123.45")
 */
export function formatCurrency(amount: number): string {
  return `$${amount.toFixed(2)}`;
}

/**
 * Formats a number as USD currency using locale formatting
 * @param amount - The amount to format
 * @returns Formatted currency string (e.g., "$1,234.56")
 */
export function formatCurrencyLocale(amount: number): string {
  return amount.toLocaleString('en-US', {
    style: 'currency',
    currency: 'USD'
  });
}
