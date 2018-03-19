using System.Collections.Generic;
using System.Linq;
using NodaTime;

namespace HealthParse.Standard.Health.Sheets.Records
{
    public class StepBuilder : IRawSheetBuilder<LocalDate>, IMonthlySummaryBuilder<LocalDate>, ISummarySheetBuilder<(int Year, int Month)>
    {
        private readonly IEnumerable<(LocalDate Date, int Steps)> _stepsByDay;
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
                    steps = new Column<LocalDate>(){ Header = ColumnNames.Steps()},
                },
                (cols, step) =>
                {
                    cols.dateColumn.Add(step.Date);
                    cols.steps.Add(step.Date, step.Steps);

                    return cols;
                });

            return new Dataset<LocalDate>(columns.dateColumn, columns.steps);
        }

        public IEnumerable<Column<(int Year, int Month)>> BuildSummary()
        {
            var stepsColumn = _stepsByDay
                .GroupBy(s => (Year: s.Date.Year, Month: s.Date.Month))
                .Aggregate(new Column<(int Year, int Month)> {Header = ColumnNames.Steps(), RangeName = "steps"},
                    (cols, step) =>
                    {
                        cols.Add(step.Key, step.Sum(r => r.Steps));
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

        private static IEnumerable<(LocalDate Date, int Steps)> GetStepsByDay(IEnumerable<Record> records, DateTimeZone zone)
        {
            return StepHelper.PrioritizeSteps(records)
                .Select(r => new { zoned = r.StartDate.InZone(zone), r })
                .GroupBy(s => s.zoned.Date)
                .Select(x => (Date: x.Key, Steps: (int)x.Sum(r => r.r.Value.SafeParse(0))))
                .OrderByDescending(s => s.Date)
                ;
        }
    }
}
