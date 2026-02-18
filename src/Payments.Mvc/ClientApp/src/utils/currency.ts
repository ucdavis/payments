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
