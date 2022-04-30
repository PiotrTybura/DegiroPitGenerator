using System;
using System.Collections.Generic;
using Integrations.Degiro.Models;

namespace Integrations.Degiro.Comparers
{
    interface ISameDividendRowComparer : IEqualityComparer<CsvCashOperation>
    {

    }

    class SameDividendRowComparer : ISameDividendRowComparer
    {
        public bool Equals(CsvCashOperation x, CsvCashOperation y)
        {
                return y != null &&
                       x != null &&
                       x.Isin == y.Isin &&
                       x.Date == y.Date &&
                       x.ExecutionDate == y.ExecutionDate &&
                       x.ChangeCurrency == y.ChangeCurrency &&
                       x.Product == y.Product;
        }

        public int GetHashCode(CsvCashOperation obj)
        {
            return Tuple.Create(obj.Isin, obj.Date, obj.ExecutionDate, obj.ChangeCurrency, obj.Product).GetHashCode();
        }
    }
}
