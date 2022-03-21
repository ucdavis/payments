using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Payments.Core.Helpers;
using Payments.Core.Models.Configuration;
using Payments.Core.Models.Sloth;

namespace Payments.Core.Services
{
    public interface ISlothService
    {
        Task<Transaction> GetTransactionsByProcessorId(string id);

        Task<IList<Transaction>> GetTransactionsByKfsKey(string kfskey);

        Task<CreateSlothTransactionResponse> CreateTransaction(CreateTransaction transaction);
    }

    public class SlothService : ISlothService
    {
        private readonly SlothSettings _settings;

        public SlothService(IOptions<SlothSettings> settings)
        {
            _settings = settings.Value;
        }

        public async Task<Transaction> GetTransactionsByProcessorId(string id)
        {
            using (var client = GetHttpClient())
            {
                var escapedId = Uri.EscapeDataString(id);
                var url = $"transactions/processor/{escapedId}";

                var response = await client.GetAsync(url);
                var result = await response.GetContentOrNullAsync<Transaction>();
                return result;
            }
        }

        public async Task<IList<Transaction>> GetTransactionsByKfsKey(string kfskey)
        {
            using (var client = GetHttpClient())
            {
                var escapedKey = Uri.EscapeDataString(kfskey);
                var url = $"transactions/kfskey/{escapedKey}";

                var response = await client.GetAsync(url);
                var result = await response.GetContentOrNullAsync<IList<Transaction>>();
                return result;
            }
        }

        public async Task<CreateSlothTransactionResponse> CreateTransaction(CreateTransaction transaction)
        {
            using (var client = GetHttpClient())
            {
                var url = "transactions";

                var response = await client.PostAsJsonAsync(url, transaction);
                var result = await response.GetContentOrNullAsync<CreateSlothTransactionResponse>();
                return result;
            }
        }

        private HttpClient GetHttpClient()
        {
            var client = new HttpClient()
            {
                BaseAddress = new Uri(_settings.BaseUrl),
            };
            client.DefaultRequestHeaders.Add("X-Auth-Token", _settings.ApiKey);

            return client;
        }
    }

    public class CreateSlothTransactionResponse
    {
        public string Id { get; set; }
    }
}
