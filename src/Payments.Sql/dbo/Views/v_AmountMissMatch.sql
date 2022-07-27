CREATE VIEW dbo.v_AmountMissMatch
AS
SELECT dbo.Invoices.Id, dbo.Teams.Name, dbo.Teams.Slug, dbo.Invoices.Status, dbo.Invoices.CalculatedTotal, dbo.Invoices.Paid, dbo.Invoices.Refunded, dbo.PaymentEvents.ProcessorId, dbo.PaymentEvents.Amount, dbo.PaymentEvents.OccuredAt, dbo.Invoices.CalculatedTotal - dbo.PaymentEvents.Amount AS diff, dbo.PaymentEvents.Decision
FROM   dbo.Invoices INNER JOIN
         dbo.Teams ON dbo.Invoices.TeamId = dbo.Teams.Id LEFT OUTER JOIN
         dbo.PaymentEvents ON dbo.Invoices.Id = dbo.PaymentEvents.InvoiceId AND dbo.Invoices.CalculatedTotal <> dbo.PaymentEvents.Amount AND dbo.Invoices.CalculatedTotal <> dbo.PaymentEvents.Amount
WHERE (dbo.Invoices.Paid = 1) AND (dbo.Invoices.Refunded = 0) AND (dbo.PaymentEvents.Decision = N'ACCEPT')
GO
EXECUTE sp_addextendedproperty @name = N'MS_DiagramPaneCount', @value = 1, @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'VIEW', @level1name = N'v_AmountMissMatch';


GO
EXECUTE sp_addextendedproperty @name = N'MS_DiagramPane1', @value = N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[48] 4[25] 2[9] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "Invoices (dbo)"
            Begin Extent = 
               Top = 12
               Left = 76
               Bottom = 650
               Right = 470
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "PaymentEvents (dbo)"
            Begin Extent = 
               Top = 84
               Left = 731
               Bottom = 538
               Right = 1289
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "Teams (dbo)"
            Begin Extent = 
               Top = 12
               Left = 1365
               Bottom = 259
               Right = 1719
            End
            DisplayFlags = 280
            TopColumn = 0
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
      Begin ColumnWidths = 13
         Width = 284
         Width = 705
         Width = 2033
         Width = 750
         Width = 750
         Width = 750
         Width = 2265
         Width = 750
         Width = 750
         Width = 750
         Width = 750
         Width = 750
         Width = 750
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 1440
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'VIEW', @level1name = N'v_AmountMissMatch';

