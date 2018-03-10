using System.Collections.Generic;
using System.Linq;
using HealthParse.Standard.Health.Sheets.Records;
using HealthParse.Standard.Health.Sheets.Workouts;
using NodaTime;

namespace HealthParse.Standard.Health.Sheets
{
    public class SummaryBuilder : IRawSheetBuilder<LocalDate>
    {
        private readonly IEnumerable<LocalDate> _healthMonths;
        private readonly IEnumerable<Column<LocalDate>> _columns;

        public SummaryBuilder(IEnumerable<Record> records,
            IEnumerable<Workout> workouts,
            DateTimeZone zone,
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
            var recordMonths = records
                .GroupBy(s => new { s.StartDate.InZone(zone).Year, s.StartDate.InZone(zone).Month })
                .Select(g => g.Key);

            var workoutMonths = workouts
                .GroupBy(s => new { s.StartDate.InZone(zone).Year, s.StartDate.InZone(zone).Month })
                .Select(g => g.Key);

            _healthMonths = recordMonths.Concat(workoutMonths)
                .Distinct()
                .Select(m => new LocalDate(m.Year, m.Month, 1));

            _columns = Enumerable.Empty<Column<LocalDate>>()
                    .Concat(stepBuilder.BuildSummary())
                    .Concat(bodyFatBuilder.BuildSummary())
                    .Concat(generalRecordsBuilder.BuildSummary())
                    .Concat(healthMarkersBuilder.BuildSummary())
                    .Concat(massBuilder.BuildSummary())
                    .Concat(distanceCyclingBuilder.BuildSummary())
                    .Concat(cyclingBuilder.BuildSummary())
                    .Concat(playBuilder.BuildSummary())
                    .Concat(ellipticalBuilder.BuildSummary())
                    .Concat(walkingBuilder.BuildSummary())
                    .Concat(runningBuilder.BuildSummary())
                    .Concat(strengthBuilder.BuildSummary())
                    .Concat(hiitBuilder.BuildSummary())
                ;
        }

        public Dataset<LocalDate> BuildRawSheet()
        {
            return new Dataset<LocalDate>(
                new KeyColumn<LocalDate>(_healthMonths) { Header = ColumnNames.Month() },
                _columns.ToArray());
        }
    }
}
