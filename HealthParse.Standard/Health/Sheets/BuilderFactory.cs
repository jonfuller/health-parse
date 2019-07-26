using HealthParse.Standard.Health.Sheets.Records;
using HealthParse.Standard.Health.Sheets.Workouts;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HealthParse.Standard.Health.Sheets
{
    public static class BuilderFactory
    {
        public static IEnumerable<(object builder, string sheetName, bool omitEmptyColumns)> GetBuilders(Settings.Settings settings, DateTimeZone zone, IList<Record> records, IList<Workout> workouts)
        {
            var stepBuilder = new StepBuilder(records, zone);
            var distanceCyclingBuilder = new DistanceCyclingBuilder(records, zone, settings);
            var massBuilder = new MassBuilder(records, zone, settings);
            var bodyFatBuilder = new BodyFatPercentageBuilder(records, zone);
            var generalRecordsBuilder = new GeneralRecordsBuilder(records, zone, settings);
            var healthMarkersBuilder = new HealthMarkersBuilder(records, zone);
            var nutritionBuilder = new NutritionBuilder(records, zone, settings);
            var settingsBuilder = new SettingsSheetBuilder(settings);

            var workoutBuilderFactory = new WorkoutBuilderFactory(workouts, zone, settings);

            var summaryBuilder = new SummaryBuilder(records, workouts, workoutBuilderFactory, zone,
                stepBuilder,
                generalRecordsBuilder,
                healthMarkersBuilder,
                nutritionBuilder,
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
                        workoutBuilderFactory,
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
                .Concat(workoutBuilderFactory.GetWorkoutBuilders().Select(builder =>
                    new{builder = (object)builder, sheetName = SheetNames.For(builder.WorkoutKey), omitEmptyColumns = true}))
                .Concat(new { builder = (object)settingsBuilder, sheetName = SheetNames.Settings, omitEmptyColumns = true })
                .ToList();

            return sheetBuilders.Select(s => (s.builder, s.sheetName, s.omitEmptyColumns));
        }
    }
}
