using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ietws;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Payments.Mvc.Models;
using Serilog;

namespace Payments.Mvc.Services
{
    public interface IDirectorySearchService
    {
        Task<Person> GetByEmail(string email);
        Task<DirectoryResult> GetByKerberos(string kerb);

    }

    public class IetWsSearchService : IDirectorySearchService
    {
        private readonly IetClient ietClient;

        public IetWsSearchService(IOptions<Settings> configuration)
        {
            var settings = configuration.Value;
            ietClient = new IetClient(settings.IetWsKey);
        }


        public async Task<Person> GetByEmail(string email)
        {
            // find the contact via their email
            var ucdContactResult = await ietClient.Contacts.Search(ContactSearchField.email, email);
            EnsureResponseSuccess(ucdContactResult);
            var ucdContact = ucdContactResult.ResponseData.Results.First();

            // now look up the whole person's record by ID including kerb
            var ucdKerbResult = await ietClient.Kerberos.Search(KerberosSearchField.iamId, ucdContact.IamId);
            EnsureResponseSuccess(ucdKerbResult);
            //TODO: If we use this method. Change the result to return DirectoryResult like GetByKerberous below.

            if (ucdKerbResult.ResponseData.Results.Length > 1)
            {
                Log.ForContext("result", ucdKerbResult, true).Warning("User search returned multiple results.");
            }

            var ucdKerbPerson = ucdKerbResult.ResponseData.Results.First();

            var additionalEmails = await GetAdditionalEmails(ucdKerbPerson.IamId, ucdContact.Email);

            return new Person
            {
                GivenName = ucdKerbPerson.DFirstName,
                Surname = ucdKerbPerson.DLastName,
                FullName = ucdKerbPerson.DFullName,
                Kerberos = ucdKerbPerson.UserId,
                Mail = ucdContact.Email,
                AdditionalEmails = additionalEmails
            };
        }

        public async Task<DirectoryResult> GetByKerberos(string kerb)
        {
            var ucdKerbResult = await ietClient.Kerberos.Search(KerberosSearchField.userId, kerb);
            EnsureResponseSuccess(ucdKerbResult);
            if (ucdKerbResult.ResponseData.Results.Length == 0)
            {
                return new DirectoryResult()
                {
                    IsInvalid = true,
                    ErrorMessage = "Login id not found. Please make sure you are using a personal login id."
                };
            }

            if (ucdKerbResult.ResponseData.Results.Length > 1)
            {
                Log.ForContext("result", ucdKerbResult, true).Warning("User search returned multiple results.");
            }

            var ucdKerbPerson = ucdKerbResult.ResponseData.Results.First();

            // find their email
            var ucdContactResult = await ietClient.Contacts.Get(ucdKerbPerson.IamId);
            EnsureResponseSuccess(ucdContactResult);
            var ucdContact = ucdContactResult.ResponseData.Results.First();

            var additionalEmails = await GetAdditionalEmails(ucdKerbPerson.IamId, ucdContact.Email);

            return new DirectoryResult()
            {
                Person = new Person()
                {
                    GivenName = ucdKerbPerson.DFirstName,
                    Surname = ucdKerbPerson.DLastName,
                    FullName = ucdKerbPerson.DFullName,
                    Kerberos = ucdKerbPerson.UserId,
                    Mail = ucdContact.Email,
                    AdditionalEmails = additionalEmails
                }
            };
        }

        private async Task<string> GetAdditionalEmails(string IamId, string primaryEmail)
        {
            try
            {
                var hsResult = await ietClient.HsData.Search(HsDataSearchField.iamId, IamId);
                EnsureResponseSuccess(hsResult);
                var additionalEmails = new List<string>();
                foreach (var hsData in hsResult.ResponseData.Results)
                {
                    if (string.IsNullOrWhiteSpace(hsData.HealthEmail))
                    {
                        continue;
                    }
                    if (additionalEmails.Contains(hsData.HealthEmail, StringComparer.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                    if (!string.Equals(hsData.HealthEmail, primaryEmail, StringComparison.OrdinalIgnoreCase))
                    {
                        additionalEmails.Add(hsData.HealthEmail);
                    }
                }


                return string.Join(";", additionalEmails);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Failed to get additional emails for IamId {IamId}", IamId);
                return string.Empty;
            }
        }

        private void EnsureResponseSuccess<T>(IetResult<T> result)
        {
            if (result.ResponseStatus != 0)
            {
                throw new ApplicationException(result.ResponseDetails);
            }
        }
    }
}
