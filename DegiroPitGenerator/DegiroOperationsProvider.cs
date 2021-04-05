using System;
using System.IO;
using System.Threading.Tasks;
using DegiroPitGenerator.Models.Configuration;
using Integrations.Degiro;
using Integrations.Degiro.Adapters;
using Integrations.Degiro.Models;
using Integrations.Degiro.Models.Configuration;
using Microsoft.Extensions.Configuration;
using Models.Operations;

namespace DegiroPitGenerator
{
    internal class DegiroOperationsProvider
    {
        private readonly DegiroConfiguration _configuration;
        private readonly ITransactionAdapter _transactionAdapter;
        private readonly IDividendAdapter _dividendAdapter;
        private readonly IFeeAdapter _feeAdapter;

        internal DegiroOperationsProvider()
        {
            _configuration = Configuration.GetSection<DegiroConfiguration>();
            _transactionAdapter = new TransactionAdapter(_configuration);
            _dividendAdapter = new DividendAdapter(_configuration);
            _feeAdapter = new FeeAdapter(_configuration);
        }

        internal async Task<YearOperations> GetYearOperations(int pitYear)
        {
            var degiroIntegration = await new Integrations.Degiro.IntegrationFactory(_configuration).Create();

            var localCsvConfiguration = Configuration.GetSection<DegiroCsvOverride>();

            ICsv<CsvTransaction> transactionCsv;
            ICsv<CsvCashOperation> cashOperationCsv;

            if (localCsvConfiguration.UseLocalCsvs == true)
            {
                transactionCsv = new Csv<CsvTransaction>(File.ReadAllText(localCsvConfiguration.TransactionsCsvPath));
                cashOperationCsv = new Csv<CsvCashOperation>(File.ReadAllText(localCsvConfiguration.CashOperationsCsvPath));
            }
            else
            {
                transactionCsv = degiroIntegration.GetTransactions();
                cashOperationCsv = degiroIntegration.GetCashOperations(pitYear);
            }
                
            var degiroTransactions = transactionCsv.GetRows();
            var degiroCashOperations = cashOperationCsv.GetRows();

            return new YearOperations
            {
                Year = pitYear,
                AllTransactions = _transactionAdapter.Adapt(degiroTransactions),
                YearDividends = _dividendAdapter.Adapt(degiroCashOperations),
                YearFees = _feeAdapter.Adapt(degiroCashOperations)
            };
        }
    }
}
