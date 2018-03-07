using System.Collections.Generic;
using System.Linq;
using NodaTime;
using UnitsNet;

namespace HealthParse.Standard.Health.Sheets.Records
{
    public class MassBuilder : IRawSheetBuilder<unit>, IMonthlySummaryBuilder<LocalDate>, ISummarySheetBuilder<LocalDate>
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
                .Select(Weight.FromRecord);
        }

        public Dataset<unit> BuildRawSheet()
        {
            var columns = _records
                .OrderByDescending(r => r.StartDate)
                .Aggregate(new
                    {
                        date = new Column<unit> { Header = ColumnNames.Date() },
                        mass = new Column<unit> { Header = ColumnNames.Weight(_settings.WeightUnit) },
                    },
                    (cols, r) =>
                    {
                        cols.date.Add(unit.v, r.StartDate.InZone(_zone));
                        cols.mass.Add(unit.v, r.Value.As(_settings.WeightUnit));
                        return cols;
                    });

            return new Dataset<unit>(columns.date, columns.mass);
        }

        public IEnumerable<Column<LocalDate>> BuildSummary()
        {
            var massColumn = _records
                .GroupBy(r => r.StartDate.InZone(_zone).Date)
                .Select(g => new { date = g.Key, mass = g.Min(x => x.Value) })
                .GroupBy(s => new { s.date.Year, s.date.Month })
                .Aggregate(new Column<LocalDate> { Header = ColumnNames.AverageWeight(_settings.WeightUnit), RangeName = "mass"},
                    (col, r) =>
                    {
                        col.Add(new LocalDate(r.Key.Year, r.Key.Month, 1), r.Average(c => c.mass).As(_settings.WeightUnit));
                        return col;
                    });

            yield return massColumn;
        }

        public IEnumerable<Column<LocalDate>> BuildSummaryForDateRange(IRange<ZonedDateTime> dateRange)
        {
            var massColumn = _records
                .Where(r => dateRange.Includes(r.StartDate.InZone(_zone), Clusivity.Inclusive))
                .GroupBy(r => r.StartDate.InZone(_zone).Date)
                .Select(g => new { date = g.Key, mass = g.Min(x => x.Value) })
                .Aggregate(new Column<LocalDate> { Header = ColumnNames.Weight(_settings.WeightUnit), RangeName = "mass"},
                    (col, r) =>
                    {
                        col.Add(r.date, r.mass.As(_settings.WeightUnit));
                        return col;
                    });

            yield return massColumn;
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
    }
}