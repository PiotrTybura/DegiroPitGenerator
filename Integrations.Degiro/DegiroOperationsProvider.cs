using System.IO;
using System.Threading.Tasks;
using Integrations.Degiro.Adapters;
using Integrations.Degiro.Models;
using Integrations.Degiro.Models.Configuration;
using Integrations.Interfaces;
using Models.Operations;

namespace Integrations.Degiro
{
    internal class DegiroOperationsProvider
    {
        private readonly DegiroConfiguration _configuration;
        private readonly ITransactionAdapter _transactionAdapter;
        private readonly IDividendAdapter _dividendAdapter;
        private readonly IFeeAdapter _feeAdapter;

        internal DegiroOperationsProvider(DegiroConfiguration configuration)
        {
            _configuration = configuration;
            _transactionAdapter = new TransactionAdapter(_configuration);
            _dividendAdapter = new DividendAdapter(_configuration);
            _feeAdapter = new FeeAdapter(_configuration);
        }

        internal async Task<YearOperations> GetYearOperations(int pitYear)
        {
            var localCsvConfiguration = _configuration.DegiroCsvOverride;

            ICsv<CsvTransaction> transactionCsv;
            ICsv<CsvCashOperation> cashOperationCsv;

            if (localCsvConfiguration.UseLocalCsvs == true)
            {
                transactionCsv = new DegiroCsv<CsvTransaction>(File.ReadAllText(localCsvConfiguration.TransactionsCsvPath));
                cashOperationCsv = new DegiroCsv<CsvCashOperation>(File.ReadAllText(localCsvConfiguration.CashOperationsCsvPath));
            }
            else
            {
                var degiroIntegration = await new IntegrationFactory(_configuration).Create();

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
