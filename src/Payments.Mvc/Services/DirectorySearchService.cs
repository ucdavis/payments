using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ietws;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Payments.Mvc.Models;

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
            var ucdKerbPerson = ucdKerbResult.ResponseData.Results.Single();
            return new Person
            {
                GivenName = ucdKerbPerson.DFirstName,
                Surname = ucdKerbPerson.DLastName,
                FullName = ucdKerbPerson.DFullName,
                Kerberos = ucdKerbPerson.UserId,
                Mail = ucdContact.Email
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
            var ucdKerbPerson = ucdKerbResult.ResponseData.Results.Single();

            // find their email
            var ucdContactResult = await ietClient.Contacts.Get(ucdKerbPerson.IamId);
            EnsureResponseSuccess(ucdContactResult);
            var ucdContact = ucdContactResult.ResponseData.Results.First();

            return new DirectoryResult()
            {
                Person = new Person()
                {
                    GivenName = ucdKerbPerson.DFirstName,
                    Surname = ucdKerbPerson.DLastName,
                    FullName = ucdKerbPerson.DFullName,
                    Kerberos = ucdKerbPerson.UserId,
                    Mail = ucdContact.Email
                }
            };
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