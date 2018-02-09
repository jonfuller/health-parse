using System.Collections.Generic;
using System.Linq;
using NodaTime;

namespace HealthParse.Standard.Health.Sheets
{
    public class DistanceCyclingBuilder : ISheetBuilder<DistanceCyclingBuilder.CyclingItem>
    {
        private readonly DateTimeZone _zone;
        private readonly IEnumerable<Record> _records;

        public DistanceCyclingBuilder(IReadOnlyDictionary<string, IEnumerable<Record>> records, DateTimeZone zone)
        {
            _zone = zone;
            _records = records.ContainsKey(HKConstants.Records.DistanceCycling)
                ? records[HKConstants.Records.DistanceCycling]
                : Enumerable.Empty<Record>();
        }
        IEnumerable<object> ISheetBuilder.BuildRawSheet()
        {
            return _records
                .OrderByDescending(r => r.StartDate)
                .Select(r => new
                {
                    date = r.StartDate.InZone(_zone),
                    distance = r.Raw.Attribute("value").Value,
                    unit = r.Raw.Attribute("unit").Value
                });
        }

        IEnumerable<CyclingItem> ISheetBuilder<CyclingItem>.BuildSummary()
        {
            return _records
                .Select(record => new {zoned = record.StartDate.InZone(_zone), record})
                .GroupBy(s => new { s.zoned.Year, s.zoned.Month })
                .Select(x => new CyclingItem(x.Key.Year, x.Key.Month, x.Sum(c => c.record.Raw.Attribute("value").ValueDouble(0) ?? 0)));
        }

        IEnumerable<CyclingItem> ISheetBuilder<CyclingItem>.BuildSummaryForDateRange(IRange<ZonedDateTime> dateRange)
        {
            return _records
                .Select(record => new { zoned = record.StartDate.InZone(_zone), record })
                .Where(x => dateRange.Includes(x.zoned, Clusivity.Inclusive))
                .GroupBy(x => x.zoned.Date)
                .Select(x => new CyclingItem(x.Key, x.Sum(c => c.record.Raw.Attribute("value").ValueDouble(0) ?? 0)))
                .OrderBy(x => x.Date);
        }

        public class CyclingItem : DatedItem
        {
            public CyclingItem(LocalDate date, double distance) : base(date)
            {
                Distance = distance;
            }

            public CyclingItem(int year, int month, double distance) : base(year, month)
            {
                Distance = distance;
            }

            public double Distance { get; }
        }
    }
}
