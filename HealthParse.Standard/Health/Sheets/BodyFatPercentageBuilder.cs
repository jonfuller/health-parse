using System;
using System.Collections.Generic;
using System.Linq;
using OfficeOpenXml;

namespace HealthParse.Standard.Health.Sheets
{
    public class BodyFatPercentageBuilder : ISheetBuilder<BodyFatPercentageBuilder.BodyFatItem>
    {
        private readonly Dictionary<string, IEnumerable<Record>> _records;

        public BodyFatPercentageBuilder(Dictionary<string, IEnumerable<Record>> records)
        {
            _records = records;
        }
        void ISheetBuilder.Build(ExcelWorksheet sheet)
        {
            var massRecords = _records[HKConstants.Records.BodyFatPercentage]
                .Select(r => new { Date = r.StartDate, BodyFatPct = r.Value.SafeParse(0) })
                .OrderByDescending(r => r.Date);

            sheet.WriteData(massRecords);
        }

        IEnumerable<BodyFatItem> ISheetBuilder<BodyFatItem>.BuildSummary()
        {
            return _records[HKConstants.Records.BodyFatPercentage]
                .GroupBy(r => r.StartDate.Date)
                .Select(g => new { date = g.Key, bodyFat = g.Min(x => x.Value.SafeParse(0)) })
                .GroupBy(s => new { s.date.Year, s.date.Month })
                .Select(x => new BodyFatItem(x.Key.Year, x.Key.Month, x.Average(c => c.bodyFat)));
        }

        IEnumerable<BodyFatItem> ISheetBuilder<BodyFatItem>.BuildSummaryForDateRange(IRange<DateTime> dateRange)
        {
            return _records[HKConstants.Records.BodyFatPercentage]
                .Where(r => dateRange.Includes(r.StartDate))
                .GroupBy(r => r.StartDate.Date)
                .Select(g => new { date = g.Key, bodyFat = g.Min(x => x.Value.SafeParse(0)) })
                .Select(x => new BodyFatItem(x.date, x.bodyFat));
        }
        public class BodyFatItem : DatedItem
        {
            public double BodyFatPercentage { get; }

            public BodyFatItem(int year, int month, double averageBodyFatPercentage) : base(year, month)
            {
                BodyFatPercentage = averageBodyFatPercentage;
            }

            public BodyFatItem(DateTime date, double bodyFatPercentage) : base(date)
            {
                BodyFatPercentage = bodyFatPercentage;
            }
        }
    }
}