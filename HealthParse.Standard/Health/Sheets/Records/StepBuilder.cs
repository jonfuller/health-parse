using System.Collections.Generic;
using System.Linq;
using NodaTime;

namespace HealthParse.Standard.Health.Sheets.Records
{
    public class StepBuilder : IRawSheetBuilder<LocalDate>, IMonthlySummaryBuilder<LocalDate>, ISummarySheetBuilder<LocalDate>
    {
        private readonly IEnumerable<StepItem> _stepsByDay;
        private readonly DateTimeZone _zone;

        public StepBuilder(IEnumerable<Record> records, DateTimeZone zone)
        {
            _zone = zone;
            _stepsByDay = GetStepsByDay(records.Where(r => r.Type == HKConstants.Records.StepCount), zone);
        }

        public Dataset<LocalDate> BuildRawSheet()
        {
            var columns = _stepsByDay
                .Aggregate(new
                {
                    dateColumn = new KeyColumn<LocalDate> { Header = ColumnNames.Date() },
                    steps = new Column<LocalDate>(),
                },
                (cols, step) =>
                {
                    cols.dateColumn.Add(step.Date);
                    cols.steps.Add(step.Date, step.Steps);

                    return cols;
                });

            return new Dataset<LocalDate>(columns.dateColumn, columns.steps);
        }

        public IEnumerable<Column<LocalDate>> BuildSummary()
        {
            var stepsColumn = _stepsByDay
                .GroupBy(s => new { s.Date.Year, s.Date.Month })
                .Aggregate(new Column<LocalDate> {Header = ColumnNames.Steps()},
                    (cols, step) =>
                    {
                        var date = new LocalDate(step.Key.Year, step.Key.Month, 1);
                        cols.Add(date, step.Sum(r => r.Steps));

                        return cols;
                    });

            yield return stepsColumn;
        }

        public IEnumerable<Column<LocalDate>> BuildSummaryForDateRange(IRange<ZonedDateTime> dateRange)
        {
            var stepsColumn = _stepsByDay
                .Where(x => dateRange.Includes(x.Date.AtStartOfDayInZone(_zone), Clusivity.Inclusive))
                .Aggregate(new Column<LocalDate> { Header = ColumnNames.Steps(), RangeName = "steps" },
                    (cols, step) =>
                    {
                        cols.Add(step.Date, step.Steps);

                        return cols;
                    });

            yield return stepsColumn;
        }

        private static IEnumerable<StepItem> GetStepsByDay(IEnumerable<Record> records, DateTimeZone zone)
        {
            return StepHelper.PrioritizeSteps(records)
                .Select(r => new { zoned = r.StartDate.InZone(zone), r })
                .GroupBy(s => s.zoned.Date)
                .Select(x => new StepItem{Date = x.Key, Steps = (int)x.Sum(r => r.r.Value.SafeParse(0))})
                .OrderByDescending(s => s.Date)
                ;
        }

        private class StepItem
        {
            public LocalDate Date { get; set; }
            public int Steps { get; set; }
        }
    }
}
