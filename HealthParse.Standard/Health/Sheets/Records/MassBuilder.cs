using System.Collections.Generic;
using System.Linq;
using NodaTime;
using UnitsNet;

namespace HealthParse.Standard.Health.Sheets.Records
{
    public class MassBuilder : IRawSheetBuilder<unit>, IMonthlySummaryBuilder<LocalDate>, ISummarySheetBuilder<(int Year, int Month)>
    {
        private readonly DateTimeZone _zone;
        private readonly Settings.Settings _settings;
        private readonly IEnumerable<(Instant StartDate, Mass Value)> _records;

        public MassBuilder(IEnumerable<Record> records, DateTimeZone zone, Settings.Settings settings)
        {
            _zone = zone;
            _settings = settings;
            _records = records
                .Where(r => r.Type == HKConstants.Records.BodyMass)
                .Select(r => (r.StartDate, RecordParser.Weight(r)));
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

        public IEnumerable<Column<(int Year, int Month)>> BuildSummary()
        {
            var massColumn = _records
                .GroupBy(r => r.StartDate.InZone(_zone).Date)
                .Select(g => new { date = g.Key, mass = g.Min(x => x.Value) })
                .GroupBy(s => (Year: s.date.Year, Month: s.date.Month ))
                .Aggregate(new Column<(int Year, int Month)> { Header = ColumnNames.AverageWeight(_settings.WeightUnit), RangeName = "mass"},
                    (col, r) =>
                    {
                        col.Add(r.Key, r.Average(c => c.mass).As(_settings.WeightUnit));
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
    }
}