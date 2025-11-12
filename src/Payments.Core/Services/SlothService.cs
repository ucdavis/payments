using Microsoft.Extensions.Options;
using Payments.Core.Helpers;
using Payments.Core.Models.Configuration;
using Payments.Core.Models.Sloth;
using Serilog;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Payments.Core.Services
{
    public interface ISlothService
    {
        Task<Transaction> GetTransactionsByProcessorId(string id, bool forRecharge = false);

        Task<IList<Transaction>> GetTransactionsByKfsKey(string kfskey, bool forRecharge = false);

        Task<CreateSlothTransactionResponse> CreateTransaction(CreateTransaction transaction, bool forRecharge = false);
    }

    public class SlothService : ISlothService
    {
        private readonly SlothSettings _settings;
        private readonly FinanceSettings _financeSettings;

        public SlothService(IOptions<SlothSettings> settings, IOptions<FinanceSettings> financeSettings)
        {
            _settings = settings.Value;
            _financeSettings = financeSettings.Value;
        }

        public async Task<Transaction> GetTransactionsByProcessorId(string id, bool forRecharge = false)
        {
            using (var client = GetHttpClient(forRecharge))
            {
                var escapedId = Uri.EscapeDataString(id);
                var url = $"transactions/processor/{escapedId}";

                var response = await client.GetAsync(url);
                var result = await response.GetContentOrNullAsync<Transaction>();
                return result;
            }
        }

        public async Task<IList<Transaction>> GetTransactionsByKfsKey(string kfskey, bool forRecharge = false)
        {
            using (var client = GetHttpClient(forRecharge))
            {
                var escapedKey = Uri.EscapeDataString(kfskey);
                var url = $"transactions/kfskey/{escapedKey}";

                var response = await client.GetAsync(url);
                var result = await response.GetContentOrNullAsync<IList<Transaction>>();
                return result;
            }
        }

        public async Task<CreateSlothTransactionResponse> CreateTransaction(CreateTransaction transaction, bool forRecharge = false)
        {
            using (var client = GetHttpClient(forRecharge))
            {
                var url = "transactions";

                var response = await client.PostAsJsonAsync(url, transaction);
                var result = await response.GetContentOrNullAsync<CreateSlothTransactionResponse>();
                return result;
            }
        }

        private HttpClient GetHttpClient(bool forRecharge)
        {
            if (_settings.BaseUrl.EndsWith("v1/", StringComparison.OrdinalIgnoreCase) ||
                _settings.BaseUrl.EndsWith("v2/", StringComparison.OrdinalIgnoreCase))
            {
                Log.Error("Sloth BaseUrl should not end with version");
                //Replace the end of the string
                _settings.BaseUrl = _settings.BaseUrl.Substring(0, _settings.BaseUrl.Length - 3);
            }
            var client = new HttpClient()
            {
                BaseAddress = new Uri($"{_settings.BaseUrl}v2/")
            };
            if (forRecharge)
            {
                client.DefaultRequestHeaders.Add("X-Auth-Token", _settings.RechargeApiKey); //This is where the magic happens
            }
            else
            {
                client.DefaultRequestHeaders.Add("X-Auth-Token", _settings.ApiKey);
            }

            return client;
        }
    }

    public class CreateSlothTransactionResponse
    {
        public string Id { get; set; }

        public string KfsTrackingNumber { get; set; } // Assigned by Sloth for recharges
    }
}
