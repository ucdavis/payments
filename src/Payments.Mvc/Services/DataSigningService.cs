using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Payments.Mvc.Models.Configuration;
using Serilog;

namespace Payments.Mvc.Services
{
    public interface IDataSigningService
    {
        string Sign(IDictionary<string, string> paramsArray);
        bool Check(IDictionary<string, string> paramsArray, string signature);
    }

    public class DataSigningService : IDataSigningService
    {
        private readonly CyberSourceSettings _cyberSourceSettings;

        public DataSigningService(IOptions<CyberSourceSettings> cyberSourceSettings)
        {
            _cyberSourceSettings = cyberSourceSettings.Value;
        }

        public string Sign(IDictionary<string, string> paramsArray)
        {
            return SignData(BuildDataToSign(paramsArray), _cyberSourceSettings.SecretKey);
        }

        public bool Check(IDictionary<string, string> paramsArray, string signature)
        {
            try
            {
                return signature == SignData(BuildDataToSign(paramsArray), _cyberSourceSettings.SecretKey);
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
                return false;
            }
        }

        private static string SignData(string data, string secretKey)
        {
            var encoding = new UTF8Encoding();
            byte[] keyByte = encoding.GetBytes(secretKey);

            var hmacsha256 = new HMACSHA256(keyByte);
            byte[] messageBytes = encoding.GetBytes(data);
            return Convert.ToBase64String(hmacsha256.ComputeHash(messageBytes));
        }

        private static string BuildDataToSign(IDictionary<string, string> paramsArray)
        {
            var signedFieldNames = paramsArray["signed_field_names"].Split(',');
            var dataToSign = signedFieldNames.Select(signedFieldName => signedFieldName + "=" + paramsArray[signedFieldName]).ToList();

            return CommaSeparate(dataToSign);
        }

        private static string CommaSeparate(IEnumerable<string> dataToSign)
        {
            return string.Join(",", dataToSign);
        }
    }
}