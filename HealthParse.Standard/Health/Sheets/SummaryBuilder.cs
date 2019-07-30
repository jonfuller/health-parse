using System.Collections.Generic;
using System.Linq;
using HealthParse.Standard.Health.Sheets.Records;
using HealthParse.Standard.Health.Sheets.Workouts;
using NodaTime;

namespace HealthParse.Standard.Health.Sheets
{
    public class SummaryBuilder : IRawSheetBuilder<(int Year, int Month)>
    {
        private readonly IEnumerable<(int Year, int Month)> _healthMonths;
        private readonly IEnumerable<Column<(int Year, int Month)>> _columns;

        public SummaryBuilder(
            IEnumerable<Record> records,
            IEnumerable<Workout> workouts,
            WorkoutBuilderFactory workoutBuilderFactory,
            DateTimeZone zone,
            StepBuilder stepBuilder,
            GeneralRecordsBuilder generalRecordsBuilder,
            HealthMarkersBuilder healthMarkersBuilder,
            NutritionBuilder nutritionBuilder,
            DistanceCyclingBuilder distanceCyclingBuilder,
            MassBuilder massBuilder,
            BodyFatPercentageBuilder bodyFatBuilder)
        {
            var workoutList = workouts.ToList();
            var recordMonths = records
                .GroupBy(s => new { s.StartDate.InZone(zone).Year, s.StartDate.InZone(zone).Month })
                .Select(g => g.Key);

            var workoutMonths = workoutList
                .GroupBy(s => new { s.StartDate.InZone(zone).Year, s.StartDate.InZone(zone).Month })
                .Select(g => g.Key);

            _healthMonths = recordMonths.Concat(workoutMonths)
                .Distinct()
                .Select(m => (Year: m.Year, Month: m.Month));

            _columns = Enumerable.Empty<Column<(int Year, int Month)>>()
                .Concat(stepBuilder.BuildSummary())
                .Concat(bodyFatBuilder.BuildSummary())
                .Concat(generalRecordsBuilder.BuildSummary())
                .Concat(healthMarkersBuilder.BuildSummary())
                .Concat(nutritionBuilder.BuildSummary())
                .Concat(massBuilder.BuildSummary())
                .Concat(distanceCyclingBuilder.BuildSummary())
                .Concat(workoutBuilderFactory.GetWorkoutBuilders().SelectMany(b => b.BuildSummary()))
                ;
        }

        public Dataset<(int Year, int Month)> BuildRawSheet()
        {
            return new Dataset<(int Year, int Month)>(
                new KeyColumn<(int Year, int Month)>(_healthMonths) { Header = ColumnNames.Month() },
                _columns.ToArray());
        }
    }
}
