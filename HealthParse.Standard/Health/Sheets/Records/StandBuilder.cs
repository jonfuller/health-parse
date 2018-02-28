using System.Collections.Generic;
using System.Linq;
using NodaTime;

namespace HealthParse.Standard.Health.Sheets.Records
{
    public class StandBuilder : IRawSheetBuilder<unit>, ISummarySheetBuilder<LocalDate>, IMonthlySummaryBuilder<LocalDate>
    {
        private readonly DateTimeZone _zone;
        private readonly List<StandDay> _dailyStandRecords;

        public StandBuilder(IEnumerable<Record> records, DateTimeZone zone)
        {
            _zone = zone;
            _dailyStandRecords = records
                .Where(r => r.Type == HKConstants.Records.Standing.StandType)
                .Where(r => r.Value == HKConstants.Records.Standing.Stood)
                .GroupBy(r => r.StartDate.InZone(_zone).Date)
                .Select(g => new StandDay{Date = g.Key,StandHours  = g.Count()})
                .OrderByDescending(r => r.Date)
                .ToList();
        }

        public Dataset<unit> BuildRawSheet()
        {
            var columns = _dailyStandRecords
                .Aggregate(new
                    {
                        date = new Column<unit> { Header = ColumnNames.Date() },
                        standing = new Column<unit> { Header = ColumnNames.StandHours() },
                    },
                    (cols, r) =>
                    {
                        cols.date.Add(unit.v, r.Date);
                        cols.standing.Add(unit.v, r.StandHours);
                        return cols;
                    });

            return new Dataset<unit>(columns.date, columns.standing);
        }

        public IEnumerable<Column<LocalDate>> BuildSummary()
        {
            var standingCol = _dailyStandRecords
                .GroupBy(s => new { s.Date.Year, s.Date.Month })
                .Aggregate(new Column<LocalDate> { Header = ColumnNames.AverageStandHours() },
                    (standing, r) =>
                    {
                        standing.Add(new LocalDate(r.Key.Year, r.Key.Month, 1), r.Average(c => c.StandHours));
                        return standing;
                    });

            yield return standingCol;
        }

        public IEnumerable<Column<LocalDate>> BuildSummaryForDateRange(IRange<ZonedDateTime> dateRange)
        {
            var standingCol = _dailyStandRecords
                .Where(r => dateRange.Includes(r.Date.AtStartOfDayInZone(_zone), Clusivity.Inclusive))
                .Aggregate(new Column<LocalDate> { Header = ColumnNames.StandHours() },
                    (standing, r) =>
                    {
                        standing.Add(r.Date, r.StandHours);
                        return standing;
                    });

            yield return standingCol;
        }

        private class StandDay
        {
            public LocalDate Date { get; set; }
            public int StandHours { get; set; }
        }
    }
}