using System;
using System.Collections.Generic;
using System.Linq;
using HealthParse.Standard.Health.Sheets.Records;
using HealthParse.Standard.Health.Sheets.Workouts;
using NodaTime;

namespace HealthParse.Standard.Health.Sheets
{
    public class MonthSummaryBuilder : IRawSheetBuilder<LocalDate>
    {
        private readonly IEnumerable<Column<LocalDate>> _columns;
        private readonly List<LocalDate> _monthDays;

        public MonthSummaryBuilder(int targetYear, int targetMonth, DateTimeZone zone,
            StepBuilder stepBuilder,
            GeneralRecordsBuilder generalRecordsBuilder,
            HealthMarkersBuilder healthMarkersBuilder,
            CyclingWorkoutBuilder cyclingBuilder,
            PlayWorkoutBuilder playBuilder,
            EllipticalWorkoutBuilder ellipticalBuilder,
            RunningWorkoutBuilder runningBuilder,
            WalkingWorkoutBuilder walkingBuilder,
            StrengthTrainingBuilder strengthBuilder,
            HiitBuilder hiitBuilder,
            DistanceCyclingBuilder distanceCyclingBuilder,
            MassBuilder massBuilder,
            BodyFatPercentageBuilder bodyFatBuilder)
        {
            _monthDays = Enumerable.Range(1, DateTime.DaysInMonth(targetYear, targetMonth))
                .Select(d => new LocalDate(targetYear, targetMonth, d))
                .ToList();

            var range = new DateRange(_monthDays.First().AtStartOfDayInZone(zone), _monthDays.Last().AtStartOfDayInZone(zone));

            _columns = Enumerable.Empty<Column<LocalDate>>()
                .Concat(stepBuilder.BuildSummaryForDateRange(range))
                .Concat(bodyFatBuilder.BuildSummaryForDateRange(range))
                .Concat(generalRecordsBuilder.BuildSummaryForDateRange(range))
                .Concat(healthMarkersBuilder.BuildSummaryForDateRange(range))
                .Concat(massBuilder.BuildSummaryForDateRange(range))
                .Concat(distanceCyclingBuilder.BuildSummaryForDateRange(range))
                .Concat(cyclingBuilder.BuildSummaryForDateRange(range))
                .Concat(playBuilder.BuildSummaryForDateRange(range))
                .Concat(ellipticalBuilder.BuildSummaryForDateRange(range))
                .Concat(walkingBuilder.BuildSummaryForDateRange(range))
                .Concat(runningBuilder.BuildSummaryForDateRange(range))
                .Concat(strengthBuilder.BuildSummaryForDateRange(range))
                .Concat(hiitBuilder.BuildSummaryForDateRange(range))
                ;
        }

        public Dataset<LocalDate> BuildRawSheet()
        {
            return new Dataset<LocalDate>(
                new KeyColumn<LocalDate>(_monthDays){Header = ColumnNames.Date()},
                _columns.ToArray());
        }
    }
}