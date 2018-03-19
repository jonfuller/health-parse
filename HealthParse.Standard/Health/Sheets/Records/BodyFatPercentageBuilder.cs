using System.Collections.Generic;
using System.Linq;
using NodaTime;

namespace HealthParse.Standard.Health.Sheets.Records
{
    public class BodyFatPercentageBuilder : IRawSheetBuilder<unit>, IMonthlySummaryBuilder<LocalDate>, ISummarySheetBuilder<(int Year, int Month)>
    {
        private readonly DateTimeZone _zone;
        private readonly IEnumerable<Record> _records;

        public BodyFatPercentageBuilder(IEnumerable<Record> records, DateTimeZone zone)
        {
            _zone = zone;
            _records = records
                .Where(r => r.Type == HKConstants.Records.BodyFatPercentage)
                .ToList();
        }
        public Dataset<unit> BuildRawSheet()
        {
            var columns = _records
                .Aggregate(new
                    {
                        date = new Column<unit> {Header = ColumnNames.Date()},
                        bodyfat = new Column<unit> {Header = ColumnNames.BodyFatPercentage()},
                    },
                    (cols, r) =>
                    {
                        cols.date.Add(unit.v, r.StartDate.InZone(_zone));
                        cols.bodyfat.Add(unit.v, r.Value.SafeParse(0));
                        return cols;
                    });

            return new Dataset<unit>(columns.date, columns.bodyfat);
        }

        public IEnumerable<Column<(int Year, int Month)>> BuildSummary()
        {
            var column = _records
                .GroupBy(r => r.StartDate.InZone(_zone).Date)
                .Select(g => new {date = g.Key, bodyFat = g.Min(x => x.Value.SafeParse(0))})
                .GroupBy(s => (Year: s.date.Year, Month: s.date.Month))
                .Aggregate(new Column<(int Year, int Month)> {Header = ColumnNames.AverageBodyFatPercentage(), RangeName = "avg_bodyfatpct"},
                    (col, r) =>
                    {
                        col.Add(r.Key, r.Average(c => c.bodyFat));
                        return col;
                    });

            yield return column;
        }

        public IEnumerable<Column<LocalDate>> BuildSummaryForDateRange(IRange<ZonedDateTime> dateRange)
        {
            var bodyFat = new Column<LocalDate> {Header = ColumnNames.BodyFatPercentage(), RangeName = "bodyfatpct" };

            _records
                .Where(r => dateRange.Includes(r.StartDate.InZone(_zone), Clusivity.Inclusive))
                .GroupBy(r => r.StartDate.InZone(_zone).Date)
                .Select(g => new { date = g.Key, bodyFat = g.Min(x => x.Value.SafeParse(0)) })
                .ToList().ForEach(f =>
                {
                    bodyFat.Add(f.date, f.bodyFat);
                });

            yield return bodyFat;
        }
    }
}