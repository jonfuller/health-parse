using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using HealthParse.Standard.Health.Export;
using HealthParse.Standard.Health.Sheets;
using HealthParse.Standard.Health.Sheets.Records;
using HealthParse.Standard.Health.Sheets.Workouts;
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
            var edt = DateTimeZone.ForOffset(Offset.FromHours(-5));
            var zone = edt;

            BuilderFactory.GetBuilders(settings, zone, records, workouts)
                .Select(b => new { b.sheetName, b.builder })
                .AsParallel().AsOrdered()
                .Select(b => new { b.sheetName, data = GetData(b.builder) })
                .AsSequential()
                .ToList().ForEach(s =>
                {
                    var sheet = workbook.Worksheets.Add(s.sheetName);
                    var wroteData = WriteData(sheet, s.data);

                    if (settings.OmitEmptySheets && !wroteData)
                    {
                        workbook.Worksheets.Delete(sheet);
                    }
                });

            workbook.PlaceCustomSheets(
                settings.CustomSheetsPlacement,
                customSheets,
                SheetNames.Summary);
        }

        private static object GetData(object builder)
        {
            var builderTypes = builder
                .GetType()
                .GetInterfaces()
                .Where(t => t.IsGenericType)
                .Single(t => t.GetGenericTypeDefinition() == typeof(IRawSheetBuilder<>))
                .GetGenericArguments();

            var openGetSheet = typeof(ExcelReport).GetMethod(nameof(GetRawSheetTyped), BindingFlags.Static | BindingFlags.NonPublic);
            var closedGetSheet = openGetSheet.MakeGenericMethod(builderTypes);

            return closedGetSheet.Invoke(null, new[] {builder});
        }

        private static Dataset<T> GetRawSheetTyped<T>(IRawSheetBuilder<T> builder)
        {
            return builder.BuildRawSheet();
        }

        private static bool WriteData(ExcelWorksheet sheet, object data)
        {
            var builderTypes = data
                .GetType()
                .GetGenericArguments();

            var openWriteSheet = typeof(ExcelReport).GetMethod(nameof(WriteSheetTyped), BindingFlags.Static | BindingFlags.NonPublic);
            var closedWriteSheet = openWriteSheet.MakeGenericMethod(builderTypes);

            return (bool)closedWriteSheet.Invoke(null, new[] { data, sheet });
        }

        private static bool WriteSheetTyped<T>(Dataset<T> sheetData, ExcelWorksheet sheet)
        {
            if (sheetData.Any())
            {
                sheet.WriteData(sheetData);
                return true;
            }

            return false;
        }
    }
}
