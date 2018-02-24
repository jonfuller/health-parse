using System.Collections.Generic;
using System.Linq;
using NodaTime;
using OfficeOpenXml;
using UnitsNet;

namespace HealthParse.Standard.Health.Sheets.Records
{
    public class MassBuilder : ISheetBuilder<MassBuilder.MassItem>
    {
        private readonly DateTimeZone _zone;
        private readonly Settings.Settings _settings;
        private readonly IEnumerable<Weight> _records;

        public MassBuilder(IEnumerable<Record> records, DateTimeZone zone, Settings.Settings settings)
        {
            _zone = zone;
            _settings = settings;
            _records = records
                .Where(r => r.Type == HKConstants.Records.BodyMass)
                .Select(Weight.FromRecord)
                .ToList();
        }

        IEnumerable<object> ISheetBuilder.BuildRawSheet()
        {
            return _records
                .Select(r => new {Date = r.StartDate.InZone(_zone).ToDateTimeUnspecified(), Mass = r.Value.As(_settings.WeightUnit)})
                .OrderByDescending(r => r.Date);
        }

        void ISheetBuilder.Customize(ExcelWorksheet _, ExcelWorkbook workbook)
        {
        }

        IEnumerable<string> ISheetBuilder.Headers => new []
        {
            ColumnNames.Date(),
            ColumnNames.Weight(_settings.WeightUnit),
        };

        IEnumerable<MassItem> ISheetBuilder<MassItem>.BuildSummary()
        {
            return _records
                .GroupBy(r => r.StartDate.InZone(_zone).Date)
                .Select(g => new{date = g.Key, mass = g.Min(x => x.Value)})
                .GroupBy(s => new { s.date.Year, s.date.Month })
                .Select(x => new MassItem(x.Key.Year, x.Key.Month, x.Average(c => c.mass)));
        }

        IEnumerable<MassItem> ISheetBuilder<MassItem>.BuildSummaryForDateRange(IRange<ZonedDateTime> dateRange)
        {
            return _records
                .Where(r => dateRange.Includes(r.StartDate.InZone(_zone), Clusivity.Inclusive))
                .GroupBy(r => r.StartDate.InZone(_zone).Date)
                .Select(g => new { date = g.Key, mass = g.Min(x => x.Value) })
                .Select(x => new MassItem(x.date, x.mass));
        }

        private class Weight
        {
            public Instant StartDate { get; private set; }
            public Mass Value { get; private set; }
            public static Weight FromRecord(Record record)
            {
                return new Weight
                {
                    StartDate = record.StartDate,
                    Value = RecordParser.Weight(record)
                };
            }
        }

        public class MassItem : DatedItem
        {
            public Mass Mass { get; }

            public MassItem(int year, int month, Mass averageMass) : base(year, month)
            {
                Mass = averageMass;
            }

            public MassItem(LocalDate date, Mass mass) : base(date)
            {
                Mass = mass;
            }
        }
    }
}