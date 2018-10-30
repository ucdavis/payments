using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Payments.Core.Data;
using Payments.Core.Models.Configuration;
using Payments.Core.Services;
using Serilog;
using SparkPost;

namespace Payments.Core.Jobs
{
    public class TaxReportJob
    {
        public static string JobName = "TaxReport";

        private readonly ApplicationDbContext _dbContext;
        private readonly IEmailService _emailService;
        private readonly PaymentsApiSettings _paymentApiSettings;

        public TaxReportJob(ApplicationDbContext dbContext, IEmailService emailService, IOptions<PaymentsApiSettings> paymentApiSettings)
        {
            _dbContext = dbContext;
            _emailService = emailService;
            _paymentApiSettings = paymentApiSettings.Value;
        }

        public async Task EmailMonthlyTaxReport(ILogger log)
        {
            try
            {
                // fetch report
                using (var client = GetHttpClient())
                {
                    var response = await client.GetAsync("ReportServices/TaxReport");
                    if (!response.IsSuccessStatusCode)
                    {
                        log.ForContext("statusCode", response.StatusCode, true)
                            .Error("Response was unsucessful");
                        return;
                    }

                    var stream = await response.Content.ReadAsByteArrayAsync();

                    var attachment = new Attachment()
                    {
                        Name = "report.xlsx",
                        Type = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        Data = Convert.ToBase64String(stream),
                    };

                    var recipient = new Recipient()
                    {
                        Address = new Address("jpknoll@ucdavis.edu", "John Knoll"),
                    };

                    await _emailService.SendTaxReport(recipient, attachment);
                }

                log.Information("Finishing Job");
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                log.Error(ex, ex.Message);
                throw;
            }
        }

        private HttpClient GetHttpClient()
        {
            return new HttpClient()
            {
                BaseAddress = new Uri(_paymentApiSettings.BaseUrl),
                DefaultRequestHeaders =
                {
                    { "X-Auth-Token", _paymentApiSettings.ApiKey },
                },
            };
        }
    }
}
