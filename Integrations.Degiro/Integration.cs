using System;
using System.Text;
using Integrations.Degiro.Models;
using Integrations.Degiro.Models.Configuration;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace Integrations.Degiro
{
    public interface IIntegration
    {
        ICsv<CsvTransaction> GetTransactions();
        ICsv<CsvCashOperation> GetCashOperations(int year);
    }

    internal class Integration : IIntegration
    {
        private readonly RestClient _client;
        private readonly RequestsConfiguration _configuration;
        private readonly string _jSessionId;
        private readonly string _accountId;

        public Integration(RequestsConfiguration configuration, string jSessionId)
        {
            _configuration = configuration;
            _jSessionId = jSessionId;
            _client = new RestClient();
            _accountId = GetAccountId();

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public ICsv<CsvCashOperation> GetCashOperations(int year)
        {
            var startDate = new DateTime(year, 1, 1);
            var endDate = new DateTime(year, 12, 31);

            return DownloadCsv<CsvCashOperation>(_configuration.CashOperationsUrl, startDate, endDate);
        }

        public ICsv<CsvTransaction> GetTransactions()
        {
            //This date needs to be before 1st transaction.
            //Since Degiro was founded in 2008 I assume that there should be no transaction before that.
            var startDate = new DateTime(2007, 1, 1);

            var endDate = DateTime.Now.AddDays(1);

            return DownloadCsv<CsvTransaction>(_configuration.TransactionsUrl, startDate, endDate);
        }

        private ICsv<T> DownloadCsv<T>(string url, DateTime startDate, DateTime endDate)
        {
            const string dateFormat = "dd'%2F'MM'%2F'yyyy";

            var request = new RestRequest(
                string.Format(url, _accountId, _jSessionId, startDate.ToString(dateFormat), endDate.ToString(dateFormat)));

            return new Csv<T>(Encoding.UTF8.GetString(_client.DownloadData(request)));
        }

        private string GetAccountId()
        {
            var request = new RestRequest(string.Format(_configuration.AccountUrl, _jSessionId));
            return JObject.Parse(_client.Execute(request).Content)["data"]["intAccount"].ToString();
        }
    }
}