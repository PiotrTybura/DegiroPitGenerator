using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;

namespace Integrations.Interfaces
{
    public interface ICsv<T>
    {
        List<T> GetRows();
    }

    public class Csv<T> : ICsv<T>
    {
        protected List<T> _records;

        public Csv(string csv)
        {
            using var stringReader = new StringReader(csv);

            using var csvReader = new CsvReader(stringReader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                //Depending on language version headers may be named differently.
                //Therefore the most reasonable way is to just base on columns order.
                HeaderValidated = null
            });

            _records = csvReader.GetRecords<T>().ToList();
        }

        public List<T> GetRows()
        {
            return _records;
        }
    }
}
