using HealthParse.Standard.Health.Sheets;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using HealthParse.Standard.Settings;

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
                var export = LoadExportXml(inputStream);

                BuildReport(export, excelFile.Workbook, settings, customSheets);

                excelFile.SaveAs(outputStream);

                return outputStream.ToArray();
            }
        }

        private static XDocument LoadExportXml(Stream exportZip)
        {
            return ZipUtilities.ReadArchive(
                exportZip,
                entry => entry.FullName == "apple_health_export/export.xml",
                entry => XDocument.Load(entry.Open()))
            .FirstOrDefault();
        }

        public static void BuildReport(XDocument export, ExcelWorkbook workbook, Settings.Settings settings, IEnumerable<ExcelWorksheet> customSheets)
        {
            var records = export.Descendants("Record")
                .Select(Record.FromXElement)
                .GroupBy(r => r.Type)
                .ToDictionary(g => g.Key, g => g.AsEnumerable());

            var workouts = export.Descendants("Workout")
                .Select(Workout.FromXElement)
                .GroupBy(r => r.WorkoutType)
                .ToDictionary(g => g.Key, g => g.AsEnumerable());

            var stepBuilder = new StepBuilder(records);
            var cyclingWorkoutBuilder = new CyclingWorkoutBuilder(workouts);
            var runningWorkoutBuilder = new RunningWorkoutBuilder(workouts);
            var walkingWorkoutBuilder = new WalkingWorkoutBuilder(workouts);
            var strengthTrainingBuilder = new StrengthTrainingBuilder(workouts);
            var distanceCyclingBuilder = new DistanceCyclingBuilder(records);
            var massBuilder = new MassBuilder(records);
            var bodyFatBuilder = new BodyFatPercentageBuilder(records);
            var settingsBuilder = new SettingsSheetBuilder(settings);

            var summaryBuilder = new SummaryBuilder(records, workouts,
                stepBuilder,
                cyclingWorkoutBuilder,
                runningWorkoutBuilder,
                walkingWorkoutBuilder,
                strengthTrainingBuilder,
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
                    var isPreviousMonth = m.Year == lastMonth.Year && m.Month == lastMonth.Year;

                    var sheetName =
                          isCurrentMonth && settings.UseConstantNameForMostRecentMonthlySummarySheet ? "Month Summary - Current"
                        : isPreviousMonth && settings.UseConstantNameForPreviousMonthlySummarySheet ? "Month Summary - Previous"
                        : $"Month Summary - {m.Year} - {m.Month}";

                    return new
                    {
                        builder = (ISheetBuilder)new MonthSummaryBuilder(m.Year,
                        m.Month,
                        stepBuilder,
                        cyclingWorkoutBuilder,
                        runningWorkoutBuilder,
                        walkingWorkoutBuilder,
                        strengthTrainingBuilder,
                        distanceCyclingBuilder,
                        massBuilder,
                        bodyFatBuilder),
                        sheetName,
                        omitEmptyColumns = settings.OmitEmptyColumnsOnMonthlySummary,
                    };
                });

            var sheetBuilders = new[] { new { builder = (ISheetBuilder)summaryBuilder, sheetName = "Overall Summary", omitEmptyColumns = settings.OmitEmptyColumnsOnOverallSummary} }
                .Concat(monthBuilders)
                .Concat(new { builder = (ISheetBuilder)stepBuilder, sheetName = "Steps", omitEmptyColumns = true })
                .Concat(new { builder = (ISheetBuilder)massBuilder, sheetName = "Mass (Weight)", omitEmptyColumns = true })
                .Concat(new { builder = (ISheetBuilder)bodyFatBuilder, sheetName = "Body Fat %", omitEmptyColumns = true })
                .Concat(new { builder = (ISheetBuilder)distanceCyclingBuilder, sheetName = "Cycling (Distance)", omitEmptyColumns = true })
                .Concat(new { builder = (ISheetBuilder)cyclingWorkoutBuilder, sheetName = "Cycling (Workouts)", omitEmptyColumns = true })
                .Concat(new { builder = (ISheetBuilder)strengthTrainingBuilder, sheetName = "Strength Training", omitEmptyColumns = true })
                .Concat(new { builder = (ISheetBuilder)runningWorkoutBuilder, sheetName = "Running", omitEmptyColumns = true })
                .Concat(new { builder = (ISheetBuilder)walkingWorkoutBuilder, sheetName = "Walking", omitEmptyColumns = true })
                .Concat(new { builder = (ISheetBuilder)settingsBuilder, sheetName = "Settings", omitEmptyColumns = true });

            sheetBuilders.ToList().ForEach(s =>
            {
                var sheetData = s.builder.BuildRawSheet().ToList();
                var keepEmptySheets = !settings.OmitEmptySheets;

                if (keepEmptySheets || sheetData.Any())
                {
                    var sheet = workbook.Worksheets.Add(s.sheetName);
                    sheet.WriteData(sheetData, omitEmptyColumns: s.omitEmptyColumns);
                }
            });

            foreach (var customSheet in customSheets)
            {
                workbook.Worksheets.Add(customSheet.Name, customSheet);
            }
        }
    }
}
