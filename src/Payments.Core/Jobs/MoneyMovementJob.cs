using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Payments.Core.Data;
using Payments.Core.Domain;
using Payments.Core.Helpers;
using Payments.Core.Models.Configuration;
using Payments.Core.Models.History;
using Payments.Core.Models.Notifications;
using Payments.Core.Models.Sloth;
using Payments.Core.Resources;
using Payments.Core.Services;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Payments.Core.Jobs
{
    public class MoneyMovementJob
    {
        public static string JobName = "MoneyMovement";

        private readonly ApplicationDbContext _dbContext;

        private readonly ISlothService _slothService;
        private readonly INotificationService _notificationService;
        private readonly FinanceSettings _financeSettings;
        private readonly PaymentsApiSettings _paymentsApiSettings;

        public MoneyMovementJob(ApplicationDbContext dbContext, ISlothService slothService, INotificationService notificationService, IOptions<FinanceSettings> financeSettings, IOptions<PaymentsApiSettings> paymentsApiSettings)
        {
            _dbContext = dbContext;
            _slothService = slothService;
            _notificationService = notificationService;
            _paymentsApiSettings = paymentsApiSettings.Value;

            _financeSettings = financeSettings.Value;
        }

        public async Task FindBankReconcileTransactions(ILogger log)
        {
            if (_financeSettings.DisableJob)
            {
                log.Information("Money Movement Job Disabled");
                return;
            }

            using (var ts = _dbContext.Database.BeginTransaction())
            {
                try
                {
                    // get all invoices that are waiting for reconcile
                    var invoices = _dbContext.Invoices
                        .Where(i => i.Status == Invoice.StatusCodes.Paid && i.Type != Invoice.InvoiceTypes.Recharge)
                        .Include(i => i.Account)
                        .Include(i => i.Team)
                        .ThenInclude(t => t.Accounts)
                        .ToList();

                    log.Information("{count} invoices found expecting reconciliation", invoices.Count);

                    foreach (var invoice in invoices)
                    {
                        //this has been changed to return a list of transactions
                        var transactions = await _slothService.GetTransactionsByProcessorId(invoice.PaymentProcessorId);
                        var transaction = transactions?.FirstOrDefault(t => string.Equals(t.Status, "Completed", StringComparison.OrdinalIgnoreCase));
                        if (transaction == null)
                        {
                            log.Warning("No reconciliation found for invoice id: {id} Paid Date: {PaidAt}", invoice.Id, invoice.PaidAt);
                            continue;
                        }
                        //This if can't happen anymore, but we don't really care.
                        if (transaction.Status != "Completed")
                        {
                            log.Warning("No completed reconciliation found for invoice id: {id} Paid Date: {PaidAt} Status: {status}", invoice.Id, invoice.PaidAt, transaction.Status);
                            continue;
                        }

                        log.Information("Invoice {id} reconciliation found with transaction: {transactionId}",
                            invoice.Id, transaction.Id);

                        // get team account info
                        var team = invoice.Team;
                        if (team.DefaultAccount == null)
                        {
                            log.Warning("Team {team} has no default account for payments", team.Name);
                            continue;
                        }

                        if (String.IsNullOrWhiteSpace(team.DefaultAccount.FinancialSegmentString))
                        {
                            log.Warning("Team {team} has no financial segment string for payments", team.Name);
                            continue;
                        }


                        // transaction found, bank reconcile was successful
                        invoice.KfsTrackingNumber = transaction.KfsTrackingNumber;
                        invoice.Status = Invoice.StatusCodes.Processing;

                        // calculate fees
                        var feeAmount = Math.Round(invoice.CalculatedTotal * FeeSchedule.StandardRate, 2);
                        var incomeAmount = invoice.CalculatedTotal - feeAmount;

                        //Setup transfers with base values
                        var debitHolding = new CreateTransfer()
                        {
                            Amount = invoice.CalculatedTotal,
                            Direction = Transfer.CreditDebit.Debit,
                            Description = "Funds Distribution",
                        };
                        var feeCredit = new CreateTransfer()
                        {
                            Amount = feeAmount,
                            Direction = Transfer.CreditDebit.Credit,
                            Description = "Processing Fee"
                        };
                        var incomeCredit = new CreateTransfer()
                        {
                            Amount = incomeAmount,
                            Direction = Transfer.CreditDebit.Credit,
                            Description = "Funds Distribution"
                        };


                        var incomeAeAccount = team.DefaultAccount.FinancialSegmentString;

                        if (invoice.Account != null && !string.IsNullOrWhiteSpace(invoice.Account.FinancialSegmentString) && invoice.Account.IsActive)
                        {
                            //Validate? here and if invalid use the team default?                            
                            // the invoice has a specified account, use it instead of the team's default
                            incomeAeAccount = invoice.Account.FinancialSegmentString;
                        }

                        // Populate transfers with financial segment strings
                        debitHolding.FinancialSegmentString = _financeSettings.ClearingFinancialSegmentString;
                        feeCredit.FinancialSegmentString = _financeSettings.FeeFinancialSegmentString;
                        incomeCredit.FinancialSegmentString = incomeAeAccount;




                        // setup transaction
                        var merchantUrl = $"https://payments.ucdavis.edu/{invoice.Team.Slug}/invoices/details/{invoice.Id}";

                        var slothTransaction = new CreateTransaction()
                        {
                            AutoApprove = _financeSettings.AutoApprove,
                            ValidateFinancialSegmentStrings = _financeSettings.ValidateFinancialSegmentString,
                            MerchantTrackingNumber = transaction.MerchantTrackingNumber,
                            MerchantTrackingUrl = merchantUrl,
                            KfsTrackingNumber = transaction.KfsTrackingNumber,
                            TransactionDate = DateTime.UtcNow,
                            Description = $"Funds Distribution INV {invoice.GetFormattedId()}",
                            Transfers = new List<CreateTransfer>()
                            {
                                debitHolding,
                                feeCredit,
                                incomeCredit,
                            },
                            Source = "Payments",
                            SourceType = "CyberSource",
                        };

                        if (feeCredit.Amount <= 0)
                        {
                            slothTransaction.Transfers = new List<CreateTransfer>()
                            {
                                debitHolding,
                                incomeCredit,
                            };
                            log.Warning("Invoice {id} Fee amount is less than or equal to 0. Removing fee transfer from transaction.", invoice.Id);
                        }

                        try
                        {
                            slothTransaction.AddMetadata("Team Name", team.Name);
                            slothTransaction.AddMetadata("Team Slug", team.Slug);
                            slothTransaction.AddMetadata("Invoice", invoice.GetFormattedId());
                        }
                        catch
                        {
                            log.Warning("Error parsing invoice meta data for invoice {id}", invoice.Id);
                        }

                        var response = await _slothService.CreateTransaction(slothTransaction);


                        //TODO: If there was a problem with the response, set the status back to paid?
                        log.Information("Transaction created with ID: {id}", response.Id);

                        // send notifications
                        try
                        {
                            var notification = new ReconcileNotification()
                            {
                                InvoiceId = invoice.Id,
                            };
                            await _notificationService.SendReconcileNotification(notification);
                        }
                        catch (Exception ex)
                        {
                            log.Error(ex, "Error while sending notification");
                        }
                    }

                    log.Information("Finishing Job");
                    await _dbContext.SaveChangesAsync();
                    ts.Commit();
                }
                catch (Exception ex)
                {
                    log.Error(ex, ex.Message);
                    ts.Rollback();
                    throw;
                }
            }
        }

        public async Task FindIncomeTransactions(ILogger log)
        {
            if (_financeSettings.DisableJob)
            {
                log.Information("Money Movement Job Disabled");
                return;
            }

            using (var ts = _dbContext.Database.BeginTransaction())
            {
                try
                {
                    // get all invoices are in processing
                    var invoices = _dbContext.Invoices
                        .Where(i => i.Status == Invoice.StatusCodes.Processing && i.Type != Invoice.InvoiceTypes.Recharge)
                        .Include(i => i.Team)
                        .ToList();

                    log.Information("{count} invoices found expecting completion", invoices.Count);

                    foreach (var invoice in invoices)
                    {
                        if (string.IsNullOrWhiteSpace(invoice.KfsTrackingNumber))
                        {
                            log.Warning("Invoice {id} has no kfs tracking number.", invoice.Id);
                            continue;
                        }

                        var transactions = await _slothService.GetTransactionsByKfsKey(invoice.KfsTrackingNumber);

                        // look for transfers into the fees account that have completed
                        // TODO: Use the sloth transaction.Description to identify these? It depends on what we write there
                        var distribution = transactions?.FirstOrDefault(t =>
                            string.Equals(t.Status, "Completed", StringComparison.OrdinalIgnoreCase)
                            && t.Transfers.Any(r => string.Equals(r.Account, _financeSettings.FeeAccount) || string.Equals(r.FinancialSegmentString, _financeSettings.FeeFinancialSegmentString)));

                        if (distribution == null && invoice.CalculatedTotal <= 0.10m)
                        {
                            //Ok, these are a special circumstance. If the invoice is less than 10 cents, we don't expect a fee.
                            //The ClearingFinancialSegmentString should have a purpose code of 00 where the CyberSource txns should have a value of 45 so they should be different.
                            distribution = transactions?.FirstOrDefault(t =>
                              string.Equals(t.Status, "Completed", StringComparison.OrdinalIgnoreCase)
                              && t.Transfers.Any(r => string.Equals(r.FinancialSegmentString, _financeSettings.ClearingFinancialSegmentString)));
                        }

                        if (distribution == null)
                        {
                            log.Warning("No reconciliation found for invoice id: {id} Paid Date: {PaidAt}", invoice.Id, invoice.PaidAt);
                            continue;
                        }

                        log.Information("Invoice {id} distribution found with transaction: {transactionId}",
                            invoice.Id, distribution.Id);

                        // transaction found, bank reconcile was successful
                        invoice.Status = Invoice.StatusCodes.Completed;
                    }

                    log.Information("Finishing Job");
                    await _dbContext.SaveChangesAsync();
                    ts.Commit();
                }
                catch (Exception ex)
                {
                    log.Error(ex, ex.Message);
                    ts.Rollback();
                    throw;
                }
            }
        }

        public async Task ProcessRechargeTransactions(ILogger log)
        {
            if (_financeSettings.RechargeDisableJob)
            {
                log.Information("Recharge Money Movement Job Disabled");
                return;
            }

            log.Information("Starting ProcessRechargeTransactions Job");
            try
            {
                var invoices = _dbContext.Invoices
                    .Where(i => i.Status == Invoice.StatusCodes.Approved && i.Type == Invoice.InvoiceTypes.Recharge)
                    .Include(i => i.Team)
                    .Include(i => i.RechargeAccounts)
                    .ToList();
                log.Information("{count} invoices found expecting upload to sloth", invoices.Count);

                foreach (var invoice in invoices)
                {
                    using (var ts = _dbContext.Database.BeginTransaction()) //Should this be for each invoice? If it fails we might get multiple uploads. (Can pass the link as the processor id as a catch...
                    {
                        try
                        {

                            var slothChecks = await _slothService.GetTransactionsByProcessorId(invoice.GetFormattedId(), true);
                            var slothCheck = slothChecks?.Where(a => a.Status != "Cancelled").FirstOrDefault(); //PendingApproval, Scheduled, Processing, Rejected, Completed 
                            if (slothCheck != null)
                            {
                                log.Warning("Invoice {id} already has a sloth transaction with processor id {processorId}. Skipping creation.", invoice.Id, invoice.GetFormattedId());
                                //It probably has an incorrect status. Lets fix it.
                                invoice.Status = Invoice.StatusCodes.Processing;
                                invoice.KfsTrackingNumber = slothCheck.KfsTrackingNumber;
                                await _dbContext.SaveChangesAsync();
                                ts.Commit();
                                continue;
                            }


                            var creditTransfers = new List<CreateTransfer>();
                            var debitTransfers = new List<CreateTransfer>();

                            foreach (var recharge in invoice.RechargeAccounts.Where(a => a.Direction == RechargeAccount.CreditDebit.Credit))
                            {
                                var creditTransfer = new CreateTransfer()
                                {
                                    Amount = recharge.Amount,
                                    Direction = Transfer.CreditDebit.Credit,
                                    Description = $"Recharge Credit INV {invoice.GetFormattedId()}",
                                };

                                creditTransfer.FinancialSegmentString = recharge.FinancialSegmentString;

                                creditTransfers.Add(creditTransfer);
                            }

                            foreach (var recharge in invoice.RechargeAccounts.Where(a => a.Direction == RechargeAccount.CreditDebit.Debit))
                            {
                                var debitTransfer = new CreateTransfer()
                                {
                                    Amount = recharge.Amount,
                                    Direction = Transfer.CreditDebit.Debit,
                                    Description = $"Recharge Debit INV {invoice.GetFormattedId()}",
                                };
                                debitTransfer.FinancialSegmentString = recharge.FinancialSegmentString;
                                debitTransfers.Add(debitTransfer);
                            }

                            if (debitTransfers.Sum(t => t.Amount) != creditTransfers.Sum(t => t.Amount))
                            {
                                log.Error("Invoice {id} debit and credit amounts do not match. Debits: {debits} Credits: {credits}", invoice.Id, debitTransfers.Sum(t => t.Amount), creditTransfers.Sum(t => t.Amount));
                                invoice.Status = Invoice.StatusCodes.Rejected;

                                var actionEntry = new History()
                                {
                                    Type = HistoryActionTypes.RechargeRejected.TypeCode,
                                    ActionDateTime = DateTime.UtcNow,
                                    Data = "Invoice debit and credit amounts do not match."
                                };
                                invoice.History.Add(actionEntry);

                                await _dbContext.SaveChangesAsync();
                                ts.Commit();
                                continue;
                            }

                            // setup transaction
                            var merchantUrl = $"{_paymentsApiSettings.BaseUrl}/{invoice.Team.Slug}/invoices/details/{invoice.Id}";
                            var payPageUrl = $"{_paymentsApiSettings.BaseUrl}/recharge/pay/{invoice.LinkId}";
                            var slothTransaction = new CreateTransaction()
                            {
                                AutoApprove = _financeSettings.RechargeAutoApprove,
                                ValidateFinancialSegmentStrings = _financeSettings.ValidateRechargeFinancialSegmentString,
                                MerchantTrackingNumber = invoice.Id.ToString(), //use the id here so these get tied together in sloth
                                MerchantTrackingUrl = merchantUrl,
                                TransactionDate = DateTime.UtcNow,
                                Description = $"Recharge INV {invoice.GetFormattedId()}",
                                Source = _financeSettings.RechargeSlothSourceName,
                                SourceType = "Recharge",
                                KfsTrackingNumber = !string.IsNullOrWhiteSpace(invoice.KfsTrackingNumber) ? invoice.KfsTrackingNumber : null, // Will be set when processed in sloth, we will get it from the response
                                Transfers = debitTransfers.Concat(creditTransfers).ToList(),
                                ProcessorTrackingNumber = invoice.GetFormattedId(),
                            };
                            slothTransaction.AddMetadata("Team Name", invoice.Team.Name);
                            slothTransaction.AddMetadata("Team Slug", invoice.Team.Slug);
                            slothTransaction.AddMetadata("Invoice", invoice.GetFormattedId());
                            slothTransaction.AddMetadata("Payment Link", payPageUrl);
                            foreach (var recharge in invoice.RechargeAccounts.Where(a => a.Direction == RechargeAccount.CreditDebit.Debit))
                            {
                                slothTransaction.AddMetadata(recharge.FinancialSegmentString, $"Entered By: {recharge.EnteredByName} ({recharge.EnteredByKerb})");
                                slothTransaction.AddMetadata(recharge.FinancialSegmentString, $"Approved By: {recharge.ApprovedByName} ({recharge.ApprovedByKerb})");
                                if (!string.IsNullOrWhiteSpace(recharge.Notes))
                                {
                                    slothTransaction.AddMetadata(recharge.FinancialSegmentString, $"Notes: {recharge.Notes}");
                                }
                            }

                            try
                            {
                                // create transaction (But before we do this, lets try to get it by processor id to avoid duplicates)
                                var response = await _slothService.CreateTransaction(slothTransaction, true);

                                if (response == null || response.Id == null)
                                {
                                    log.Error("Invoice {id} sloth transaction creation failed.", invoice.Id);
                                    continue;
                                }
                                invoice.Status = Invoice.StatusCodes.Processing;
                                invoice.KfsTrackingNumber = response.KfsTrackingNumber;
                                await _dbContext.SaveChangesAsync();
                                ts.Commit();
                            }
                            catch (HttpServiceInternalException ex) when (ex.StatusCode == System.Net.HttpStatusCode.BadRequest)
                            {
                                log.Error(ex, "Bad request creating sloth transaction for invoice {id}. Message: {message}, Content: {content}",
                                    invoice.Id, ex.Message, ex.Content);
                                invoice.Status = Invoice.StatusCodes.Rejected;
                                var actionEntry = new History()
                                {
                                    Type = HistoryActionTypes.RechargeRejected.TypeCode,
                                    ActionDateTime = DateTime.UtcNow,
                                    Data = $"Recharge transaction rejected by Sloth: {ex.Content}"
                                };
                                invoice.History.Add(actionEntry);
                                await _dbContext.SaveChangesAsync();
                                ts.Commit();
                                continue;
                            }
                            catch (HttpServiceInternalException ex)
                            {
                                log.Error(ex, "Error creating sloth transaction for invoice {id}. Status: {statusCode}, Content: {content}",
                                    invoice.Id, ex.StatusCode, ex.Content);
                                //Leave it in approved to try again later
                                continue;
                            }
                            catch (Exception ex)
                            {
                                log.Error(ex, "Error creating sloth transaction for invoice {id}", invoice.Id);
                                continue;
                            }
                            await _dbContext.SaveChangesAsync();
                            ts.Commit();
                        }
                        catch (Exception ex)
                        {
                            log.Error(ex, "Error processing invoice {id}", invoice.Id);
                            try
                            {
                                ts.Rollback();
                            }
                            catch (InvalidOperationException)
                            {
                                // Transaction was already rolled back or committed
                                log.Warning("Transaction for invoice {id} was already completed", invoice.Id);
                            }
                            continue;
                        }
                    }
                } //End of foreach invoice


                log.Information("Finishing ProcessRechargeTransactions Job");
            }
            catch (Exception ex)
            {
                log.Error(ex, ex.Message);
            }
        }

        public async Task ProcessRechargePendingTransactions(ILogger log)
        {
            if (_financeSettings.RechargeDisableJob)
            {
                log.Information("Recharge Money Movement Job Disabled");
                return;
            }
            log.Information("Starting ProcessRechargePendingTransactions Job");

            using (var ts = _dbContext.Database.BeginTransaction())
            {
                try
                {
                    var invoices = _dbContext.Invoices
                        .Where(i => i.Status == Invoice.StatusCodes.Processing && i.Type == Invoice.InvoiceTypes.Recharge)
                        .Include(i => i.Team)
                        .Include(i => i.RechargeAccounts) //Might want this for notifications
                        .ToList();

                    foreach (var invoice in invoices)
                    {
                        if (string.IsNullOrWhiteSpace(invoice.KfsTrackingNumber))
                        {
                            log.Warning("Invoice {id} has no kfs tracking number.", invoice.Id);
                            continue;
                        }

                        //Should only be one, but just in case...
                        var transactions = await _slothService.GetTransactionsByProcessorId(invoice.GetFormattedId(), true); //This can return multiples because we are re-using the KFS number.

                        //var slothTransaction = await _slothService.GetTransactionsByProcessorId(invoice.GetFormattedId(), true); //Could also use this way. They should both be the same info
                        // look for transfers into the fees account that have completed
                        var transaction = transactions?.FirstOrDefault(t =>
                            string.Equals(t.Status, "Completed", StringComparison.OrdinalIgnoreCase));

                        if (transaction != null)
                        {
                            log.Information("Invoice {id} recharge distribution found with transaction: {transactionId}", invoice.Id, transaction.Id);
                            // transaction found, bank reconcile was successful
                            invoice.Status = Invoice.StatusCodes.Completed;
                            //invoice.PaidAt = transaction.TransactionDate; //Going to keep this what it was to show when the user approved it.
                            if (invoice.PaidAt == null)
                            {
                                invoice.PaidAt = transaction.TransactionDate;
                            }
                            invoice.Paid = true;

                            // send notifications
                            try
                            {
                                var notification = new ReconcileNotification()
                                {
                                    InvoiceId = invoice.Id,
                                };
                                await _notificationService.SendReconcileNotification(notification);
                            }
                            catch (Exception ex)
                            {
                                log.Error(ex, "Error while sending notification");
                            }
                            //await _dbContext.SaveChangesAsync();

                        }
                        else
                        {
                            transaction = transactions?.FirstOrDefault(t => string.Equals(t.Status, "Cancelled", StringComparison.OrdinalIgnoreCase));
                            if (transaction != null)
                            {
                                log.Information("Invoice {id} recharge distribution cancelled with transaction: {transactionId}", invoice.Id, transaction.Id);

                                invoice.Status = Invoice.StatusCodes.Rejected;
                                var actionEntry = new History()
                                {
                                    Type = HistoryActionTypes.RechargeRejected.TypeCode,
                                    ActionDateTime = DateTime.UtcNow,
                                    Data = "Recharge transaction was cancelled in Sloth."
                                };
                                invoice.History.Add(actionEntry);
                            }
                            //await _dbContext.SaveChangesAsync();
                        }
                        //The other actionable status could be Rejected, but because that means it could be manually edited in sloth, we don't want to do anything unless it is cancelled.

                    }

                    await _dbContext.SaveChangesAsync();

                    ts.Commit();
                    log.Information("Finishing ProcessRechargePendingTransactions Job");
                }
                catch (Exception ex)
                {
                    //TODO: Review this
                    log.Error(ex, ex.Message);
                    ts.Rollback();
                    throw;
                }
            }
        }
    }
}
