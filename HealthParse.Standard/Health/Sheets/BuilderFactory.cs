using System;
using System.Collections.Generic;
using HealthParse.Standard.Health.Sheets.Records;
using HealthParse.Standard.Health.Sheets.Workouts;
using NodaTime;
using System.Linq;

namespace HealthParse.Standard.Health.Sheets
{
    public static class BuilderFactory
    {
        public static IEnumerable<(object builder, string sheetName, bool omitEmptyColumns)> GetBuilders(Settings.Settings settings, DateTimeZone zone, IList<Record> records, IList<Workout> workouts)
        {
            var stepBuilder = new StepBuilder(records, zone);
            var cyclingWorkoutBuilder = new CyclingWorkoutBuilder(workouts, zone, settings);
            var playWorkoutBuilder = new PlayWorkoutBuilder(workouts, zone, settings);
            var ellipticalWorkoutBuilder = new EllipticalWorkoutBuilder(workouts, zone, settings);
            var runningWorkoutBuilder = new RunningWorkoutBuilder(workouts, zone, settings);
            var walkingWorkoutBuilder = new WalkingWorkoutBuilder(workouts, zone, settings);
            var strengthTrainingBuilder = new StrengthTrainingBuilder(workouts, zone, settings);
            var hiitBuilder = new HiitBuilder(workouts, zone, settings);
            var distanceCyclingBuilder = new DistanceCyclingBuilder(records, zone, settings);
            var massBuilder = new MassBuilder(records, zone, settings);
            var bodyFatBuilder = new BodyFatPercentageBuilder(records, zone);
            var generalRecordsBuilder = new GeneralRecordsBuilder(records, zone, settings);
            var healthMarkersBuilder = new HealthMarkersBuilder(records, zone);
            var nutritionBuilder = new NutritionBuilder(records, zone, settings);
            var settingsBuilder = new SettingsSheetBuilder(settings);

            var summaryBuilder = new SummaryBuilder(records, workouts, zone,
                stepBuilder,
                generalRecordsBuilder,
                healthMarkersBuilder,
                nutritionBuilder,
                cyclingWorkoutBuilder,
                playWorkoutBuilder,
                ellipticalWorkoutBuilder,
                runningWorkoutBuilder,
                walkingWorkoutBuilder,
                strengthTrainingBuilder,
                hiitBuilder,
                distanceCyclingBuilder,
                massBuilder,
                bodyFatBuilder);

            var monthBuilders = Enumerable.Range(0, settings.NumberOfMonthlySummaries)
                .Select(i => DateTime.Today.AddMonths(-i))
                .Select(d => new { d.Year, d.Month })
                .Select(m =>
                {
                    var lastMonth = DateTime.Today.AddMonths(-1);

                    var isCurrentMonth = m.Year == DateTime.Today.Year && m.Month == DateTime.Today.Month;
                    var isPreviousMonth = m.Year == lastMonth.Year && m.Month == lastMonth.Month;

                    var sheetName =
                          isCurrentMonth && settings.UseConstantNameForMostRecentMonthlySummarySheet ? SheetNames.MonthSummary.Current
                        : isPreviousMonth && settings.UseConstantNameForPreviousMonthlySummarySheet ? SheetNames.MonthSummary.Previous
                        : SheetNames.MonthSummary.Name(m.Year, m.Month);

                    var builder = new MonthSummaryBuilder(m.Year,
                        m.Month,
                        zone,
                        stepBuilder,
                        generalRecordsBuilder,
                        healthMarkersBuilder,
                        nutritionBuilder,
                        cyclingWorkoutBuilder,
                        playWorkoutBuilder,
                        ellipticalWorkoutBuilder,
                        runningWorkoutBuilder,
                        walkingWorkoutBuilder,
                        strengthTrainingBuilder,
                        hiitBuilder,
                        distanceCyclingBuilder,
                        massBuilder,
                        bodyFatBuilder);

                    return new
                    {
                        builder = (object)builder,
                        sheetName,
                        omitEmptyColumns = settings.OmitEmptyColumnsOnMonthlySummary,
                    };
                }).ToList();

            var sheetBuilders = new[] { new { builder = (object)summaryBuilder, sheetName = SheetNames.Summary, omitEmptyColumns = settings.OmitEmptyColumnsOnOverallSummary } }
                .Concat(monthBuilders)
                .Concat(new { builder = (object)stepBuilder, sheetName = SheetNames.Steps, omitEmptyColumns = true })
                .Concat(new { builder = (object)massBuilder, sheetName = SheetNames.Mass, omitEmptyColumns = true })
                .Concat(new { builder = (object)bodyFatBuilder, sheetName = SheetNames.BodyFat, omitEmptyColumns = true })
                .Concat(new { builder = (object)generalRecordsBuilder, sheetName = SheetNames.GeneralRecords, omitEmptyColumns = true })
                .Concat(new { builder = (object)healthMarkersBuilder, sheetName = SheetNames.HealthMarkers, omitEmptyColumns = true })
                .Concat(new { builder = (object)nutritionBuilder, sheetName = SheetNames.Nutrition, omitEmptyColumns = true })
                .Concat(new { builder = (object)distanceCyclingBuilder, sheetName = SheetNames.CyclingDistance, omitEmptyColumns = true })
                .Concat(new { builder = (object)cyclingWorkoutBuilder, sheetName = SheetNames.CyclingWorkouts, omitEmptyColumns = true })
                .Concat(new { builder = (object)strengthTrainingBuilder, sheetName = SheetNames.StrengthTraining, omitEmptyColumns = true })
                .Concat(new { builder = (object)hiitBuilder, sheetName = SheetNames.Hiit, omitEmptyColumns = true })
                .Concat(new { builder = (object)runningWorkoutBuilder, sheetName = SheetNames.Running, omitEmptyColumns = true })
                .Concat(new { builder = (object)walkingWorkoutBuilder, sheetName = SheetNames.Walking, omitEmptyColumns = true })
                .Concat(new { builder = (object)ellipticalWorkoutBuilder, sheetName = SheetNames.Elliptical, omitEmptyColumns = true })
                .Concat(new { builder = (object)playWorkoutBuilder, sheetName = SheetNames.Play, omitEmptyColumns = true })
                .Concat(new { builder = (object)settingsBuilder, sheetName = SheetNames.Settings, omitEmptyColumns = true })
                .ToList();

            return sheetBuilders.Select(s => (s.builder, s.sheetName, s.omitEmptyColumns));
        }
    }
}
