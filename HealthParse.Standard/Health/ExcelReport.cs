using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using HealthParse.Standard.Health.Export;
using HealthParse.Standard.Health.Sheets;
using HealthParse.Standard.Health.Sheets.Records;
using HealthParse.Standard.Health.Sheets.Workouts;
using HealthParse.Standard.Settings;
using NodaTime;
using OfficeOpenXml;

namespace HealthParse.Standard.Health
{
    public static class ExcelReport
    {
        public static byte[] CreateReport(byte[] exportZip, Settings.Settings settings, IEnumerable<ExcelWorksheet> customSheets)
        {
            using (var inputStream = new MemoryStream(exportZip))
            using (var outputStream = new MemoryStream())
            using (var excelFile = new ExcelPackage())
            {
                var loader = ZipUtilities.ReadArchive(
                        inputStream,
                        entry => entry.FullName == "apple_health_export/export.xml",
                        entry => new XmlReaderExportLoader(entry.Open()))
                    .FirstOrDefault();

                BuildReport(loader.Records, loader.Workouts, excelFile.Workbook, settings, customSheets);

                excelFile.SaveAs(outputStream);

                return outputStream.ToArray();
            }
        }

        public static void BuildReport(IList<Record> records, IList<Workout> workouts, ExcelWorkbook workbook, Settings.Settings settings, IEnumerable<ExcelWorksheet> customSheets)
        {
            var customSheetsList = customSheets.ToList();

            var edt = DateTimeZone.ForOffset(Offset.FromHours(-5));
            var zone = edt;
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
            var settingsBuilder = new SettingsSheetBuilder(settings);

            var summaryBuilder = new SummaryBuilder(records, workouts, zone,
                stepBuilder,
                generalRecordsBuilder,
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
                          isCurrentMonth && settings.UseConstantNameForMostRecentMonthlySummarySheet ? "Month Summary - Current"
                        : isPreviousMonth && settings.UseConstantNameForPreviousMonthlySummarySheet ? "Month Summary - Previous"
                        : $"Month Summary - {m.Year} - {m.Month}";

                    var builder = new MonthSummaryBuilder(m.Year,
                        m.Month,
                        zone,
                        stepBuilder,
                        generalRecordsBuilder,
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


            var summarySheetName = "Overall Summary";
            var sheetBuilders = new[] { new { builder = (object)summaryBuilder, sheetName = summarySheetName, omitEmptyColumns = settings.OmitEmptyColumnsOnOverallSummary } }
                .Concat(monthBuilders)
                .Concat(new { builder = (object)stepBuilder, sheetName = "Steps", omitEmptyColumns = true })
                .Concat(new { builder = (object)massBuilder, sheetName = "Mass (Weight)", omitEmptyColumns = true })
                .Concat(new { builder = (object)bodyFatBuilder, sheetName = "Body Fat %", omitEmptyColumns = true })
                .Concat(new { builder = (object)generalRecordsBuilder, sheetName = "General Records", omitEmptyColumns = true })
                .Concat(new { builder = (object)distanceCyclingBuilder, sheetName = "Cycling (Distance)", omitEmptyColumns = true })
                .Concat(new { builder = (object)cyclingWorkoutBuilder, sheetName = "Cycling (Workouts)", omitEmptyColumns = true })
                .Concat(new { builder = (object)strengthTrainingBuilder, sheetName = "Strength Training", omitEmptyColumns = true })
                .Concat(new { builder = (object)hiitBuilder, sheetName = "HIIT", omitEmptyColumns = true })
                .Concat(new { builder = (object)runningWorkoutBuilder, sheetName = "Running", omitEmptyColumns = true })
                .Concat(new { builder = (object)walkingWorkoutBuilder, sheetName = "Walking", omitEmptyColumns = true })
                .Concat(new { builder = (object)ellipticalWorkoutBuilder, sheetName = "Elliptical", omitEmptyColumns = true })
                .Concat(new { builder = (object)playWorkoutBuilder, sheetName = "Play", omitEmptyColumns = true })
                .Concat(new { builder = (object)settingsBuilder, sheetName = "Settings", omitEmptyColumns = true })
                .ToList();

            sheetBuilders
                .Where(s => s.builder != null)
                .ToList().ForEach(s => AddSheet(workbook, settings, s.builder, s.sheetName));

            workbook.PlaceCustomSheets(
                settings.CustomSheetsPlacement,
                customSheetsList,
                summarySheetName,
                monthBuilders.Select(b => b.sheetName).ToList());
        }

        private static void AddSheet(ExcelWorkbook workbook, Settings.Settings settings, object builder, string sheetName)
        {
            var builderTypes = builder
                .GetType()
                .GetInterfaces()
                .Where(t => t.IsGenericType)
                .Single(t => t.GetGenericTypeDefinition() == typeof(IRawSheetBuilder<>))
                .GetGenericArguments();

            var openAddSheet = typeof(ExcelReport).GetMethod(nameof(AddSheetTyped), BindingFlags.Static | BindingFlags.NonPublic);
            var closedAddSheet = openAddSheet.MakeGenericMethod(builderTypes);

            closedAddSheet.Invoke(null, new[] {builder, sheetName, workbook, settings});
        }

        private static void AddSheetTyped<T>(IRawSheetBuilder<T> builder, string sheetName, ExcelWorkbook workbook, Settings.Settings settings)
        {
            var sheetData = builder.BuildRawSheet();
            var keepEmptySheets = !settings.OmitEmptySheets;

            if (keepEmptySheets || sheetData.Any())
            {
                var sheet = workbook.Worksheets.Add(sheetName);

                sheet.WriteData(sheetData);
            }
        }
    }
}
