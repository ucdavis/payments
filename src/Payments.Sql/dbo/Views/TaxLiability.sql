

CREATE VIEW [dbo].[TaxLiability] AS 

SELECT  
    i.TeamId, 
    t.Name, 
    i.Id AS InvoiceId, 
    { fn CONCAT(format(i.Id, '000-'), format(i.DraftCount, '000')) } AS FormattedInvoiceId, 
    i.AccountId, 
    CASE 
        WHEN a.FinancialSegmentString IS NULL THEN { fn CONCAT(CONCAT(a.Chart, '-'), a.Account) } 
        WHEN a.Account IS NULL THEN a.FinancialSegmentString 
        ELSE { fn CONCAT(a.FinancialSegmentString, CONCAT(' (', CONCAT(CONCAT(a.Chart, '-'), CONCAT(a.Account, ')')))) } 
    END AS IncomeAccount, 
    i.PaidAt, 
    CONVERT(date, i.PaidAt) AS PaidOn, 
    i.CalculatedTotal, 
    CONVERT(bit, l_1.HasTaxExempt) AS HasTaxExemptItems, 
    i.CalculatedTaxableAmount, 
    i.TaxPercent, 
    i.CalculatedTaxAmount
FROM    dbo.Invoices AS i LEFT OUTER JOIN
           dbo.FinancialAccounts AS a ON i.AccountId = a.Id LEFT OUTER JOIN
           dbo.Teams AS t ON i.TeamId = t.Id LEFT OUTER JOIN
               (SELECT  InvoiceId, MAX(CONVERT(int, TaxExempt)) AS HasTaxExempt
              FROM     dbo.LineItems AS l
              GROUP BY InvoiceId) AS l_1 ON l_1.InvoiceId = i.Id
WHERE  (i.Paid = 1) AND (i.CalculatedTaxAmount > 0)
