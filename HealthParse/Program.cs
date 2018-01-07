using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace HealthParse
{
    class Program
    {
        
        static void Main(string[] args)
        {
            var fileLocation = "export.zip";
            //var fileLocation = "/Users/jon/Google Drive/export.zip";
            var export = ReadArchive(
                    fileLocation,
                    entry => entry.FullName == "apple_health_export/export.xml",
                    entry => XDocument.Load(entry.Open()))
                .FirstOrDefault();

            var grouped = export.Descendants("Record")
                .Select(r => {
                    var startDate = r.Attribute("startDate").ValueDateTime();
                    var endDate = r.Attribute("endDate").ValueDateTime();
                    return new {
                        type = r.Attribute("type").Value,
                        endDate = endDate,
                        startDate = startDate,
                        dateRange = new DateRange(startDate, endDate),
                        creationDate = r.Attribute("creationDate")?.ValueDateTime(),
                        value = r.Attribute("value")?.Value ?? "<null>",
                        unit = r.Attribute("unit")?.Value ?? "<null>",
                        raw = r };
                })
                .GroupBy(r => r.type)
                .ToDictionary(g => g.Key, g => g.AsEnumerable());

            //var mass = grouped["HKQuantityTypeIdentifierBodyMass"];
            //var steps = grouped["HKQuantityTypeIdentifierStepCount"]
            //    .GroupBy(s => s.startDate.Date)
            //    .Select(x => new {
            //        date = x.Key,
            //        steps = x.Sum(r => r.value.SafeParse(0))
            //    });

            //var steps2 = grouped["HKQuantityTypeIdentifierStepCount"]
                //.GroupBy(s => s.startDate.Date)
                //.SelectMany(x => {
                //    return x
                //        .GroupBy(b => b.raw.Attribute("sourceName").Value)
                //        .Select(b => new
                //        {
                //            date = x.Key,
                //            source = b.Key,
                //            steps = b.Sum(r => r.value.SafeParse(0))
                //        });
                //});

            var justSteps = grouped["HKQuantityTypeIdentifierStepCount"].ToList();
            var trimmed = justSteps.Take(0).ToList();

            for (int i = 0; i < justSteps.Count; i++)
            {
                var current = justSteps[i];
                var next = justSteps.Skip(i + 1).FirstOrDefault();
                var nextOverlaps = next != null && current.dateRange.Includes(next.startDate);

                if (nextOverlaps)
                {
                    var keeper = new[]{current, next}
                        .First(l => l.raw.Attribute("sourceName").Value.Contains("Watch"));

                    //trimmed.Add(keeper);
                    Console.WriteLine($"skipping {i}");
                    i--;
                }
                else
                {
                    trimmed.Add(current);
                }
            }

            using (var writer = new StreamWriter("steps.csv", false))
            {
                trimmed
                    .Where(r => r.endDate.Date == new DateTime(2017, 2, 25))
                    .OrderBy(r => r.endDate)
                    .Select(r => $"{r.raw.Attribute("sourceName").Value},{r.startDate},{r.endDate},{r.value}")
                    .ToList().ForEach(writer.WriteLine);
                //grouped["HKQuantityTypeIdentifierStepCount"]
                    //.Where(r => r.endDate.Date == new DateTime(2017, 2, 25))
                    //.OrderBy(r => r.endDate)
                    //.Select(r => $"{r.raw.Attribute("sourceName").Value},{r.startDate},{r.endDate},{r.value}")
                    //.ToList().ForEach(writer.WriteLine);
            }



            //Console.WriteLine(mass.Count());
            //Console.WriteLine(string.Join(Environment.NewLine, steps.Select(m => $"{m.date} {m.steps}")));
            //steps2
                //.Select(m => $"{m.date} {m.steps} ({m.source})")
                //.ToList().ForEach(Console.WriteLine);
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

}
