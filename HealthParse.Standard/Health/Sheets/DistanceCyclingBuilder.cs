using System.Collections.Generic;
using System.Linq;
using OfficeOpenXml;

namespace HealthParse.Standard.Health.Sheets
{
    public class DistanceCyclingBuilder : ISheetBuilder
    {
        private readonly Dictionary<string, IEnumerable<Record>> _records;

        public DistanceCyclingBuilder(Dictionary<string, IEnumerable<Record>> records)
        {
            _records = records;
        }
        void ISheetBuilder.Build(ExcelWorksheet sheet)
        {
            var cycling = _records[HKConstants.Records.DistanceCycling]
                .OrderByDescending(r => r.StartDate)
                .Select(r => new
                {
                    date = r.StartDate,
                    distance = r.Raw.Attribute("value").Value,
                    unit = r.Raw.Attribute("unit").Value
                });

            sheet.WriteData(cycling);
        }
    }
}
