using System;
using System.Collections.Generic;
using System.Linq;
using NodaTime;

namespace HealthParse.Standard.Health.Sheets
{
    public class BodyFatPercentageBuilder : ISheetBuilder<BodyFatPercentageBuilder.BodyFatItem>
    {
        private readonly DateTimeZone _zone;
        private readonly IEnumerable<Record> _records;

        public BodyFatPercentageBuilder(IReadOnlyDictionary<string, IEnumerable<Record>> records, DateTimeZone zone)
        {
            _zone = zone;
            _records = records.ContainsKey(HKConstants.Records.BodyFatPercentage)
                ? records[HKConstants.Records.BodyFatPercentage]
                : Enumerable.Empty<Record>();
        }
        IEnumerable<object> ISheetBuilder.BuildRawSheet()
        {
            return _records
                .Select(r => new { Date = r.StartDate.InZone(_zone), BodyFatPct = r.Value.SafeParse(0) })
                .OrderByDescending(r => r.Date.ToInstant());
        }

        IEnumerable<BodyFatItem> ISheetBuilder<BodyFatItem>.BuildSummary()
        {
            return _records
                .GroupBy(r => r.StartDate.InZone(_zone).Date)
                .Select(g => new { date = g.Key, bodyFat = g.Min(x => x.Value.SafeParse(0)) })
                .GroupBy(s => new { s.date.Year, s.date.Month })
                .Select(x => new BodyFatItem(x.Key.Year, x.Key.Month, x.Average(c => c.bodyFat)));
        }

        IEnumerable<BodyFatItem> ISheetBuilder<BodyFatItem>.BuildSummaryForDateRange(IRange<ZonedDateTime> dateRange)
        {
            return _records
                .Where(r => dateRange.Includes(r.StartDate.InZone(_zone), Clusivity.Inclusive))
                .GroupBy(r => r.StartDate.InZone(_zone).Date)
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

            public BodyFatItem(LocalDate date, double bodyFatPercentage) : base(date)
            {
                BodyFatPercentage = bodyFatPercentage;
            }
        }
    }
}