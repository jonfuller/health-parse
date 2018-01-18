using HealthParse.Standard.Health.Sheets;
using OfficeOpenXml;
using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace HealthParse.Standard.Health
{
    public static class ExcelReport
    {
        public static byte[] CreateReport(byte[] exportZip)
        {
            using (var inputStream = new MemoryStream(exportZip))
            using (var outputStream = new MemoryStream())
            using (var excelFile = new ExcelPackage())
            {
                var export = LoadExportXml(inputStream);

                BuildReport(export, excelFile.Workbook);

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

        public static void BuildReport(XDocument export, ExcelWorkbook workbook)
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

            var summaryBuilder = new SummaryBuilder(records, workouts,
                stepBuilder,
                cyclingWorkoutBuilder,
                runningWorkoutBuilder,
                walkingWorkoutBuilder,
                strengthTrainingBuilder,
                distanceCyclingBuilder);

            var monthBuilders = Enumerable.Range(0, 3)
                .Select(i => DateTime.Today.AddMonths(-i))
                .Select(d => new { d.Year, d.Month })
                .Select(m => new
                {
                    builder = (ISheetBuilder)new MonthSummaryBuilder(records, workouts,
                        m.Year, m.Month,
                        stepBuilder,
                        cyclingWorkoutBuilder,
                        runningWorkoutBuilder,
                        walkingWorkoutBuilder,
                        strengthTrainingBuilder,
                        distanceCyclingBuilder),
                    sheetName = $"Month Summary - {m.Year} - {m.Month}",
                });

            var sheetBuilders = new[] { new { builder = (ISheetBuilder)summaryBuilder, sheetName = "Overall Summary"} }
                .Concat(monthBuilders)
                .Concat(new { builder = (ISheetBuilder)stepBuilder, sheetName = "Steps" })
                .Concat(new { builder = (ISheetBuilder)distanceCyclingBuilder, sheetName = "Cycling (Distance)" })
                .Concat(new { builder = (ISheetBuilder)cyclingWorkoutBuilder, sheetName = "Cycling (Workouts)" })
                .Concat(new { builder = (ISheetBuilder)strengthTrainingBuilder, sheetName = "Strength Training" })
                .Concat(new { builder = (ISheetBuilder)runningWorkoutBuilder, sheetName = "Running" })
                .Concat(new { builder = (ISheetBuilder)walkingWorkoutBuilder, sheetName = "Walking" });

            sheetBuilders.ToList().ForEach(s =>
            {
                var sheet = workbook.Worksheets.Add(s.sheetName);
                s.builder.Build(sheet);
            });
        }
    }
}
