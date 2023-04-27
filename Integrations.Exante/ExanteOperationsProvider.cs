using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Integrations.Exante;
using Integrations.Exante.Adapters;
using Integrations.Exante.Models;
using Integrations.Interfaces;
using Models.Operations;
using Models.Operations.Fees;

namespace DegiroPitGenerator
{
    public class ExanteOperationsProvider
    {
        //private readonly ExanteConfiguration _configuration;
        private readonly ITransactionAdapter _transactionAdapter;
        private readonly IDividendAdapter _dividendAdapter;
        private readonly IFeeAdapter _feeAdapter;

        public ExanteOperationsProvider()
        {
            //_configuration = Configuration.GetSection<DegiroConfiguration>();
            _transactionAdapter = new TransactionAdapter();
            _dividendAdapter = new DividendAdapter();
            _feeAdapter = new FeeAdapter();
        }

        public async Task<YearOperations> GetYearOperations(int pitYear)
        {
            //var localCsvConfiguration = Configuration.GetSection<DegiroCsvOverride>();

            ICsv<CsvTransaction> transactionCsv;
            ICsv<CsvCashOperation> cashOperationCsv;

            //if (localCsvConfiguration.UseLocalCsvs == true)
            //{
            //   transactionCsv = new Csv<CsvTransaction>(File.ReadAllText(localCsvConfiguration.TransactionsCsvPath));
            transactionCsv = new ExanteCsv<CsvTransaction>(File.ReadAllText(@".\ExanteTransactions.csv"));
            //   cashOperationCsv = new Csv<CsvCashOperation>(File.ReadAllText(localCsvConfiguration.CashOperationsCsvPath));
            cashOperationCsv = new ExanteCsv<CsvCashOperation>(File.ReadAllText(@".\ExanteCashOperations.csv"));
            //}
            //else
            //{
            //  var degiroIntegration = await new Integrations.Degiro.IntegrationFactory(_configuration).Create();

            //  transactionCsv = degiroIntegration.GetTransactions();
            //    cashOperationCsv = degiroIntegration.GetCashOperations(pitYear);
            //}

            var exanteTransactions = transactionCsv.GetRows();
            var exanteCashOperations = cashOperationCsv.GetRows();

            return new YearOperations
            {
                Year = pitYear,
                AllTransactions = _transactionAdapter.Adapt(exanteTransactions, exanteCashOperations),
                YearDividends = _dividendAdapter.Adapt(exanteCashOperations.Where(_ => _.Date.Year == pitYear).ToList()),
                YearFees = _feeAdapter.Adapt(exanteCashOperations.Where(_ => _.Date.Year == pitYear).ToList())
            };
        }
    }
}
