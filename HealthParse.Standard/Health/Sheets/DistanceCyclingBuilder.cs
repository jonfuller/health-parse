using System.Collections.Generic;
using System.Linq;
using OfficeOpenXml;

namespace HealthParse.Standard.Health.Sheets
{
    public class DistanceCyclingBuilder : ISheetBuilder<DistanceCyclingBuilder.MonthlyCycling>
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

        IEnumerable<MonthlyCycling> ISheetBuilder<MonthlyCycling>.BuildSummary()
        {
            return _records[HKConstants.Records.DistanceCycling]
                .GroupBy(s => new { s.StartDate.Date.Year, s.StartDate.Date.Month })
                .Select(x => new MonthlyCycling(x.Key.Year, x.Key.Month, x.Sum(c => c.Raw.Attribute("value").ValueDouble(0) ?? 0)));
        }

        public class MonthlyCycling : MonthlyItem
        {
            public MonthlyCycling(int year, int month, double distance) : base(year, month)
            {
                Distance = distance;
            }

            public double Distance { get; }
        }
    }
}
