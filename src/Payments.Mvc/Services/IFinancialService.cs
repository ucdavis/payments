using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Serilog;

namespace Payments.Mvc.Services
{
    public interface IFinancialService
    {
        Task<string> GetAccountName(string chart, string account, string subAccount);
    }

    public class FinancialService : IFinancialService
    {
        private readonly Settings _settings;

        public FinancialService(IOptions<Settings> settings)
        {
            _settings = settings.Value;
        }

        public async Task<string> GetAccountName(string chart, string account, string subAccount)
        {
            //https://kfs.ucdavis.edu/kfs-prd/api-docs/ //Documentation
            string url;
            string validationUrl;
            if (!String.IsNullOrWhiteSpace(subAccount))
            {
                validationUrl =
                    $"{_settings.FinancialLookupUrl}/subaccount/{chart}/{account}/{subAccount}/isvalid";
                url =
                    $"{_settings.FinancialLookupUrl}/subaccount/{chart}/{account}/{subAccount}/name";
            }
            else
            {
                validationUrl = $"{_settings.FinancialLookupUrl}/account/{chart}/{account}/isvalid";
                url = $"{_settings.FinancialLookupUrl}/account/{chart}/{account}/name";
            }

            using (var client = new HttpClient())
            {
                var validationResponse = await client.GetAsync(validationUrl);
                validationResponse.EnsureSuccessStatusCode();

                var validationContents = await validationResponse.Content.ReadAsStringAsync();
                if (!JsonConvert.DeserializeObject<bool>(validationContents))
                {
                    Log.Information($"Account not valid {account}");
                    throw new Exception("Invalid Account");
                }


                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();


                var contents = await response.Content.ReadAsStringAsync();
                return contents;
            }

        }
    }
}
