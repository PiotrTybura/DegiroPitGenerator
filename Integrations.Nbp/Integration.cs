using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Models;
using Models.ExchangeRate;
using RestSharp;

namespace Integrations.Nbp
{
    public interface IIntegration
    {
        IEnumerable<ExchangeRate> GetExchangeRates();
    }

    public class Integration : IIntegration
    {
        private const string Url = "https://www.nbp.pl/kursy/Archiwum/archiwum_tab_a_{0}.csv";
        private readonly RestClient _client;

        public Integration()
        {
            _client = new RestClient();

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }
        public IEnumerable<ExchangeRate> GetExchangeRates()
        {
            var exchangeRates = new List<ExchangeRate>();

            //Polish Central Bank does not provide exchangeRates reports before 2012
            //Data in 2012 are presented in a different format so ExchangeRates
            //are fetched from 2013
            for(int year=2013; year <= DateTime.Now.Year; year++)
            {
                var csv = DownloadCsvFromUrl(string.Format(Url, year));
                exchangeRates.AddRange(DeserializeCsvToRates(csv));
            }

            return exchangeRates;
        }
         
        private string DownloadCsvFromUrl(string url)
        {
            var request = new RestRequest(url);
            request.AddHeader("Accept", "*/*");
            return Encoding.GetEncoding("ISO-8859-2").GetString(_client.DownloadData(request));
        }

        private IEnumerable<ExchangeRate> DeserializeCsvToRates(string csv)
        {
            //There is a risk that number of currencies will change over time so we can't use a static model for deserialisation purposes
            using var stringReader = new StringReader(csv);

            var currencies = GetCurrenciesFromHeaders(stringReader);

            while(stringReader.Peek() != -1)
            {
                var currentLine = stringReader.ReadLine();

                //Starting from 2013 there are some footer rows in csvs. They start with empty line
                if (currentLine == "")
                    break;

                var cells = currentLine.Split(';').ToList();
                var date = DateTime.ParseExact(cells.First(), "yyyyMMdd", CultureInfo.InvariantCulture);
                cells.RemoveAt(0);

                for(int i = 0; i < cells.Count - 3; i++)
                {
                    yield return new ExchangeRate
                    {
                        Date = date,
                        BaseCurrency = Enum.Parse<Currency>(currencies[i].Name),
                        //The code was prepared for Degiro accounts denominated in EUR
                        CounterCurrency = Currency.PLN,
                        Rate = decimal.Parse(cells[i]) / currencies[i].Denominator
                    };
                }
            }
        }

        private List<(string Name, int Denominator)> GetCurrenciesFromHeaders(StringReader stringReader)
        {
            const string headersRegexPattern = @"^([1-9]\d*?)([A-Z]{3})$";

            var headers = stringReader.ReadLine().Split(';').ToList();

            //FirstHeader represents date
            //Last one is empty
            //Second and first from the end represents row identifiers
            var headersToIgnore = new List<string>();
            headersToIgnore.Add(headers[0]);
            headersToIgnore.AddRange(headers.Skip(headers.Count - 3));

            //Firstly let's verify that all documents meets these requirements
            if (headersToIgnore.Any(h => Regex.IsMatch(h, headersRegexPattern))
                || headers.Distinct().Count() != headers.Count)
                throw new InvalidDataException("Csv columns have been modified and does not meet the previous standard anymore");

            //Then cut them out
            headers.RemoveAll(h => headersToIgnore.Contains(h));

            //Starting from 2013 there headers are also in 2nd row.
            //They represents currency descriptions - they are not deserialized
            stringReader.ReadLine();

            return headers.Select(h => {
                var match = Regex.Match(h, headersRegexPattern);
                return (Name: match.Groups[2].Value, Denominator: int.Parse(match.Groups[1].Value));
            }).ToList();
        }
    }
}