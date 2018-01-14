using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
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
            using (var reader = new StreamReader(inputStream))
            using (var writer = new StreamWriter(outputStream))
            {
                CreateReport(writer, reader);
                writer.Flush();
                return outputStream.ToArray();
            }
        }

        public static void CreateReport(StreamWriter output, StreamReader exportZip)
        {
            using (var p = new ExcelPackage())
            {
                var export = LoadExport(exportZip);

                BuildReport(export, p.Workbook);

                p.SaveAs(output.BaseStream);
            }
        }

        private static XDocument LoadExport(StreamReader exportZip)
        {
            return ReadArchive(
                exportZip.BaseStream,
                entry => entry.FullName == "apple_health_export/export.xml",
                entry => XDocument.Load(entry.Open()))
            .FirstOrDefault();
        }

        private static void BuildReport(XDocument export, ExcelWorkbook workbook)
        {
            var records = export.Descendants("Record")
                .Select(Record.FromXElement)
                .GroupBy(r => r.Type)
                .ToDictionary(g => g.Key, g => g.AsEnumerable());

            var workouts = export.Descendants("Workout")
                .Select(Workout.FromXElement)
                .GroupBy(r => r.WorkoutType)
                .ToDictionary(g => g.Key, g => g.AsEnumerable());

            BuildSummary(records, workouts, workbook.Worksheets.Add("Summary"));
            BuildSteps(records[HKConstants.Records.StepCount], workbook.Worksheets.Add("Steps"));
        }

        private static void BuildSummary(Dictionary<string, IEnumerable<Record>> records, Dictionary<string, IEnumerable<Workout>> workouts, ExcelWorksheet worksheet)
        {
            worksheet.Cells["A1"].Value = "Hello World";
        }

        private static void BuildSteps(IEnumerable<Record> records, ExcelWorksheet worksheet)
        {
            var steps = PrioritizeSteps(records)
                .GroupBy(s => s.StartDate.Date)
                .Select(x => new
                {
                    date = x.Key,
                    steps = x.Sum(r => r.Value.SafeParse(0))
                })
                .OrderByDescending(s => s.date);

            Write(steps, worksheet);
        }

        private static IEnumerable<Record> PrioritizeSteps(IEnumerable<Record> allTheSteps)
        {
            var justSteps = allTheSteps.OrderBy(r => r.StartDate).ToList();

            for (int i = 0; i < justSteps.Count; i++)
            {
                var current = justSteps[i];
                var next = justSteps.Skip(i + 1).FirstOrDefault();
                var nextOverlaps = next != null && current.DateRange.Includes(next.StartDate);

                if (nextOverlaps)
                {
                    var keeper = new[] { current, next }
                        .First(l => l.Raw.Attribute("sourceName").Value.Contains("Watch"));
                    var loser = new[] { current, next }.Where(x => x != keeper).Single();

                    justSteps.Remove(loser);
                    i--;
                }
                else
                {
                    yield return current;
                }
            }
        }

        private static IEnumerable<IEnumerable<string>> GetLines(IEnumerable<object> rows)
        {
            if (!rows.Any())
            {
                yield break;
            }

            var first = rows.First();
            var props = first.GetType().GetProperties();

            yield return props.Select(prop => prop.Name);
            foreach (var row in rows)
            {
                yield return props.Select(prop => prop.GetValue(row).ToString());
            }
        }

        private static void Write(IEnumerable<object> rows, ExcelWorksheet worksheet)
        {
            GetLines(rows)
                .SelectMany((row, rowNum) => row.Select((value, columnNum) => new { value, rowNum, columnNum }))
                .ToList()
                .ForEach(item => worksheet.Cells[item.rowNum + 1, item.columnNum + 1].Value = item.value);
        }

        private static IEnumerable<T> ReadArchive<T>(Stream exportZip, Func<ZipArchiveEntry, bool> entryFilter, Func<ZipArchiveEntry, T> eachEntry)
        {
            using (var archive = new ZipArchive(exportZip, ZipArchiveMode.Read, true))
            {
                foreach (var entry in archive.Entries)
                {
                    if (entryFilter(entry))
                    {
                        yield return eachEntry(entry);
                    }
                }
            }
        }
    }
}
