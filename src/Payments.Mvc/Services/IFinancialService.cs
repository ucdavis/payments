using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Payments.Mvc.Models.FinancialModels;
using Serilog;

namespace Payments.Mvc.Services
{
    public interface IFinancialService
    {
        Task<string> GetAccountName(string chart, string account, string subAccount);
        Task<KfsAccount> GetAccount(string chart, string account);
        Task<bool> IsAccountValid(string chart, string account, string subAccount);
        Task<bool> IsObjectValid(string chart, string objectCode);
        Task<bool> IsSubObjectValid(string chart, string account, string objectCode, string subObject);
        Task<bool> IsProjectValid(string project);
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

        public async Task<bool> IsAccountValid(string chart, string account, string subAccount)
        {
            string validationUrl;
            if (!String.IsNullOrWhiteSpace(subAccount))
            {
                validationUrl =
                    $"{_settings.FinancialLookupUrl}/subaccount/{chart}/{account}/{subAccount}/isvalid";
            }
            else
            {
                validationUrl = $"{_settings.FinancialLookupUrl}/account/{chart}/{account}/isvalid";
            }

            using (var client = new HttpClient())
            {
                var validationResponse = await client.GetAsync(validationUrl);
                validationResponse.EnsureSuccessStatusCode();

                var validationContents = await validationResponse.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<bool>(validationContents); //TEST THIS!!!
            }
        }

        /// <summary>
        /// Current fiscal year
        /// </summary>
        /// <param name="chart"></param>
        /// <param name="account"></param>
        /// <param name="subAccount"></param>
        /// <returns></returns>
        public async Task<bool> IsObjectValid(string chart, string objectCode)
        {
            string validationUrl = $"{_settings.FinancialLookupUrl}/object/{chart}/{objectCode}/isvalid";


            using (var client = new HttpClient())
            {
                var validationResponse = await client.GetAsync(validationUrl);
                validationResponse.EnsureSuccessStatusCode();

                var validationContents = await validationResponse.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<bool>(validationContents); //TEST THIS!!!
            }
        }

        /// <summary>
        /// GET /fau/subobject/{chart}/{account}/{object}/{subobject}/name
        /// </summary>
        /// <param name="chart"></param>
        /// <param name="account"></param>
        /// <param name="objectCode"></param>
        /// <param name="subObject"></param>
        /// <returns></returns>
        public async Task<bool> IsSubObjectValid(string chart, string account, string objectCode, string subObject)
        {
            string validationUrl = $"{_settings.FinancialLookupUrl}/subobject/{chart}/{account}/{objectCode}/{subObject}/isvalid";


            using (var client = new HttpClient())
            {
                var validationResponse = await client.GetAsync(validationUrl);
                validationResponse.EnsureSuccessStatusCode();

                var validationContents = await validationResponse.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<bool>(validationContents); //TEST THIS!!!
            }
        }

        public async Task<bool> IsProjectValid(string project)
        {
            string url = $"{_settings.FinancialLookupUrl}/project/{project}/isvalid";


            using (var client = new HttpClient())
            {
                var validationResponse = await client.GetAsync(url);
                validationResponse.EnsureSuccessStatusCode();

                var validationContents = await validationResponse.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<bool>(validationContents); //TEST THIS!!!
            }
        }

        public async Task<KfsAccount> GetAccount(string chart, string account)
        {
            string url = $"{_settings.FinancialLookupUrl}/account/{chart}/{account}";
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var contents = await response.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<KfsAccount>(contents);
            }
        }

        public async Task<string> GetProjectName(string project)
        {
            string url = $"{_settings.FinancialLookupUrl}/project/{project}";
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var contents = await response.Content.ReadAsStringAsync();
                return contents;
            }
        }
    }
}
