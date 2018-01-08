using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Linq;

namespace HealthParse
{
    class Program
    {
        static void Main(string[] args)
        {
            var fileLocation = "export.zip";
            var export = ReadArchive(
                    fileLocation,
                    entry => entry.FullName == "apple_health_export/export.xml",
                    entry => XDocument.Load(entry.Open()))
                .FirstOrDefault();

            var grouped = export.Descendants("Record")
                .Select(r => Record.FromXElement(r))
                .GroupBy(r => r.Type)
                .ToDictionary(g => g.Key, g => g.AsEnumerable());

            var dailySteps = PrioritizeSteps(grouped[HKConstants.StepCount])
                .GroupBy(s => s.StartDate.Date)
                .Select(x => new
                {
                    date = x.Key,
                    steps = x.Sum(r => r.Value.SafeParse(0))
                });

            dailySteps
                .Select(m => $"{m.date} {m.steps}")
                .ToList().ForEach(Console.WriteLine);
            Console.ReadKey();
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

        private static IEnumerable<T> ReadArchive<T>(string zipFileLocation, Func<ZipArchiveEntry, bool> entryFilter, Func<ZipArchiveEntry, T> eachEntry)
        {
            using (var reader = new StreamReader(zipFileLocation))
            using (var archive = new ZipArchive(reader.BaseStream, ZipArchiveMode.Read, true))
            {
                foreach(var entry in archive.Entries)
                {
                    if (entryFilter(entry))
                    {
                        yield return eachEntry(entry);
                    }
                }
            }
        }
    }

    public static class HKConstants
    {
        public const string BodyMass = "HKQuantityTypeIdentifierBodyMass";
        public const string StepCount = "HKQuantityTypeIdentifierStepCount";
    }

    public static class Help
    {
        public static double SafeParse(this string target, double valueIfParseFail)
        {
            double result = 0;
            var parsed = double.TryParse(target, out result);
            return parsed ? result : valueIfParseFail;
        }

        public static DateTime ValueDateTime(this XAttribute target)
        {
            return target?.Value.ToDateTime() ?? DateTime.MinValue;
        }

        public static DateTime ToDateTime(this string target)
        {
            return DateTime.Parse(target);
        }
    }
    public interface IRange<T>
    {
        T Start { get; }
        T End { get; }
        bool Includes(T value);
        bool Includes(IRange<T> range);
    }

    public class DateRange : IRange<DateTime>
    {
        public DateRange(DateTime start, DateTime end)
        {
            Start = start;
            End = end;
        }

        public DateTime Start { get; private set; }
        public DateTime End { get; private set; }

        public bool Includes(DateTime value)
        {
            return (Start < value) && (value < End);
        }

        public bool Includes(IRange<DateTime> range)
        {
            return (Start < range.Start) && (range.End < End);
        }
    }
    public class Record
    {
        public string Type { get; private set; }
        public DateTime StartDate { get; private set; }
        public DateTime EndDate { get; private set; }
        public DateTime? CreationDate { get; private set; }
        public XElement Raw { get; private set; }
        public DateRange DateRange { get; private set; }
        public string Value { get; private set; }
        public string Unit { get; private set; }
        public static Record FromXElement(XElement r)
        {
            var startDate = r.Attribute("startDate").ValueDateTime();
            var endDate = r.Attribute("endDate").ValueDateTime();
            return new Record
            {
                Type = r.Attribute("type").Value,
                EndDate = endDate,
                StartDate = startDate,
                DateRange = new DateRange(startDate, endDate),
                CreationDate = r.Attribute("creationDate")?.ValueDateTime(),
                Value = r.Attribute("value")?.Value ?? "<null>",
                Unit = r.Attribute("unit")?.Value ?? "<null>",
                Raw = r
            };
        }
    }
}
