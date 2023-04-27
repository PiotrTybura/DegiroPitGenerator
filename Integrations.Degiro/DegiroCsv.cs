using System.Collections.Generic;
using Extensions;
using Integrations.Degiro.Models;
using Integrations.Interfaces;

namespace Integrations.Degiro
{

    public class DegiroCsv<T> : Csv<T>
    {
        public DegiroCsv(string csv) : base(csv)
        {
            _records = Fix(_records);
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
