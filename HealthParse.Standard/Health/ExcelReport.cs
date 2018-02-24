using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HealthParse.Standard.Health.Export;
using HealthParse.Standard.Health.Sheets;
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
            var settingsBuilder = new SettingsSheetBuilder(settings);

            var summaryBuilder = new SummaryBuilder(records, workouts, zone, settings,
                stepBuilder,
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

                    return new
                    {
                        builder = (ISheetBuilder)new MonthSummaryBuilder(m.Year,
                        m.Month,
                        zone,
                        settings,
                        stepBuilder,
                        cyclingWorkoutBuilder,
                        playWorkoutBuilder,
                        ellipticalWorkoutBuilder,
                        runningWorkoutBuilder,
                        walkingWorkoutBuilder,
                        strengthTrainingBuilder,
                        hiitBuilder,
                        distanceCyclingBuilder,
                        massBuilder,
                        bodyFatBuilder),
                        sheetName,
                        omitEmptyColumns = settings.OmitEmptyColumnsOnMonthlySummary,
                    };
                }).ToList();


            var summarySheetName = "Overall Summary";
            var sheetBuilders = new[] { new { builder = (ISheetBuilder)summaryBuilder, sheetName = summarySheetName, omitEmptyColumns = settings.OmitEmptyColumnsOnOverallSummary} }
                .Concat(monthBuilders)
                .Concat(new { builder = (ISheetBuilder)stepBuilder, sheetName = "Steps", omitEmptyColumns = true })
                .Concat(new { builder = (ISheetBuilder)massBuilder, sheetName = "Mass (Weight)", omitEmptyColumns = true })
                .Concat(new { builder = (ISheetBuilder)bodyFatBuilder, sheetName = "Body Fat %", omitEmptyColumns = true })
                .Concat(new { builder = (ISheetBuilder)distanceCyclingBuilder, sheetName = "Cycling (Distance)", omitEmptyColumns = true })
                .Concat(new { builder = (ISheetBuilder)cyclingWorkoutBuilder, sheetName = "Cycling (Workouts)", omitEmptyColumns = true })
                .Concat(new { builder = (ISheetBuilder)strengthTrainingBuilder, sheetName = "Strength Training", omitEmptyColumns = true })
                .Concat(new { builder = (ISheetBuilder)hiitBuilder, sheetName = "HIIT", omitEmptyColumns = true })
                .Concat(new { builder = (ISheetBuilder)runningWorkoutBuilder, sheetName = "Running", omitEmptyColumns = true })
                .Concat(new { builder = (ISheetBuilder)walkingWorkoutBuilder, sheetName = "Walking", omitEmptyColumns = true })
                .Concat(new { builder = (ISheetBuilder)ellipticalWorkoutBuilder, sheetName = "Elliptical", omitEmptyColumns = true })
                .Concat(new { builder = (ISheetBuilder)playWorkoutBuilder, sheetName = "Play", omitEmptyColumns = true })
                .Concat(new { builder = (ISheetBuilder)settingsBuilder, sheetName = "Settings", omitEmptyColumns = true });

            sheetBuilders.ToList().ForEach(s =>
            {
                var sheetData = s.builder.BuildRawSheet().ToList();
                var keepEmptySheets = !settings.OmitEmptySheets;

                if (keepEmptySheets || sheetData.Any())
                {
                    var sheet = workbook.Worksheets.Add(s.sheetName);
                    sheet.WriteData(
                        sheetData,
                        omitEmptyColumns: s.omitEmptyColumns,
                        headers: s.builder.HasHeaders ? s.builder.Headers : null);
                    s.builder.Customize(sheet, workbook);
                }
            });

            foreach (var customSheet in customSheetsList)
            {
                workbook.Worksheets.Add(customSheet.Name, customSheet);
            }

            PlaceCustomSheets(
                settings.CustomSheetsPlacement,
                customSheetsList,
                workbook.Worksheets,
                summarySheetName,
                monthBuilders.Select(b => b.sheetName).ToList());
        }

        private static void PlaceCustomSheets(CustomSheetsPlacement placement, IEnumerable<ExcelWorksheet> customSheets, ExcelWorksheets sheets, string summarySheetName, IList<string> monthSummaryNames)
        {
            switch (placement)
            {
                case CustomSheetsPlacement.AfterSummary:
                    foreach (var customSheet in customSheets)
                    {
                        sheets.MoveAfter(customSheet.Name, summarySheetName);
                    }
                    break;
                case CustomSheetsPlacement.AfterMonthlySummaries:
                    if (monthSummaryNames.IsEmpty()) break;
                    var lastMonth = monthSummaryNames.Last();

                    foreach (var customSheet in customSheets)
                    {
                        sheets.MoveAfter(customSheet.Name, lastMonth);
                    }

                    break;
                case CustomSheetsPlacement.First:
                    foreach (var customSheet in customSheets)
                    {
                        sheets.MoveToStart(customSheet.Name);
                    }

                    break;
                case CustomSheetsPlacement.Last:
                default:
                    // do nothing, they're already at the end
                    break;
            }
        }
    }
}
