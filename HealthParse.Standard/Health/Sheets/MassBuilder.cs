using System;
using System.Collections.Generic;
using System.Linq;

namespace HealthParse.Standard.Health.Sheets
{
    public class MassBuilder : ISheetBuilder<MassBuilder.MassItem>
    {
        private readonly IEnumerable<Record> _records;

        public MassBuilder(IReadOnlyDictionary<string, IEnumerable<Record>> records)
        {
            _records = records.ContainsKey(HKConstants.Records.BodyMass)
                ? records[HKConstants.Records.BodyMass]
                : Enumerable.Empty<Record>();
        }
        IEnumerable<object> ISheetBuilder.BuildRawSheet()
        {
            return _records
                .Select(r => new {Date = r.StartDate, Mass = r.Value.SafeParse(0) })
                .OrderByDescending(r => r.Date);
        }

        IEnumerable<MassItem> ISheetBuilder<MassItem>.BuildSummary()
        {
            return _records
                .GroupBy(r => r.StartDate.Date)
                .Select(g => new{date = g.Key, mass = g.Min(x => x.Value.SafeParse(0))})
                .GroupBy(s => new { s.date.Year, s.date.Month })
                .Select(x => new MassItem(x.Key.Year, x.Key.Month, x.Average(c => c.mass)));
        }

        IEnumerable<MassItem> ISheetBuilder<MassItem>.BuildSummaryForDateRange(IRange<DateTime> dateRange)
        {
            return _records
                .Where(r => dateRange.Includes(r.StartDate))
                .GroupBy(r => r.StartDate.Date)
                .Select(g => new { date = g.Key, mass = g.Min(x => x.Value.SafeParse(0)) })
                .Select(x => new MassItem(x.date, x.mass));
        }

        public class MassItem : DatedItem
        {
            public double Mass { get; }

            public MassItem(int year, int month, double averageMass) : base(year, month)
            {
                Mass = averageMass;
            }

            public MassItem(DateTime date, double mass) : base(date)
            {
                Mass = mass;
            }
        }
    }
}