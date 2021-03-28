using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;
using Extensions;
using Integrations.Degiro.Models;

namespace Integrations.Degiro
{
    public interface ICsv<T>
    {
        List<T> GetRows();
    }

    public class Csv<T> : ICsv<T>
    {
        private readonly List<T> _records;

        public Csv(string csv)
        {
            using var stringReader = new StringReader(csv);

            using var csvReader = new CsvReader(stringReader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                //Depending on language version headers may be named differently.
                //Therefore the most reasonable way is to just base on columns order.
                HeaderValidated = null,
            });

            _records = Fix(csvReader.GetRecords<T>().ToList());
        }

        public List<T> GetRows()
        {
            return _records;
        }

        private List<T> Fix(List<T> records)
        {
            if (records is List<CsvTransaction> transactions)
                //There are some artifact rows in Degiro transaction csv
                transactions.RemoveAll(_ => _.TransactionId == "");
            else if(records is IEnumerable<CsvCashOperation> cashOperations)
            {
                //CsvReader does not support polish culture info, so we need to manually adjust it
                cashOperations.ForEach(_ => _.BalanceAmount /= 100m);
                cashOperations.ForEach(_ => _.ChangeAmount /= 100m);
                cashOperations.ForEach(_ => _.ExchangeRate /= 10000m);
            }

            return records;
        }
    }
}
