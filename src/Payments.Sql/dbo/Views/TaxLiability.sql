
CREATE VIEW [dbo].[TaxLiability] AS 

SELECT
	i.TeamId,
	t.Name,
	i.Id AS InvoiceId,
	CONCAT(format(i.Id, '000'), '-', format(i.DraftCount, '000')) AS FormattedInvoiceId,
	i.AccountId,
	CONCAT(a.Chart, '-', a.Account) AS IncomeAccount,
	i.PaidAt,
	i.Total,
	CONVERT(bit, l.HasTaxExempt) AS HasTaxExemptItems,
	i.TaxableAmount,
	i.TaxPercent,
	i.TaxAmount
FROM Invoices i
	left join FinancialAccounts a ON i.AccountId = a.Id
	left join Teams t ON i.TeamId = t.Id
	left join (
		SELECT
			l.InvoiceId,
			MAX(convert(int, l.TaxExempt)) AS HasTaxExempt
		FROM LineItems l GROUP BY l.InvoiceId
	) l on l.InvoiceId = i.Id
WHERE i.Paid = 1 AND i.TaxAmount > 0