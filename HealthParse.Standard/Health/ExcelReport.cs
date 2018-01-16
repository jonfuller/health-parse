using HealthParse.Standard.Health.Sheets;
using OfficeOpenXml;
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

            var sheetBuilders = new[]
            {
                new {builder = (ISheetBuilder)new SummaryBuilder(records, workouts), sheetName = "Summary" },
                new {builder = (ISheetBuilder)new StepBuilder(records), sheetName = "Steps" },
                new {builder = (ISheetBuilder)new DistanceCyclingBuilder(records), sheetName = "Cycling (Distance)" },
                new {builder = (ISheetBuilder)new CyclingWorkoutBuilder(workouts), sheetName = "Cycling (Workouts)" },
                new {builder = (ISheetBuilder)new StrengthTrainingBuilder(workouts), sheetName = "Strength Training" },
                new {builder = (ISheetBuilder)new RunningWorkoutBuilder(workouts), sheetName = "Running" },
                new {builder = (ISheetBuilder)new WalkingWorkoutBuilder(workouts), sheetName = "Walking" },
            };

            sheetBuilders.ToList().ForEach(s => s.builder.Build(workbook.Worksheets.Add(s.sheetName)));
        }
    }
}
