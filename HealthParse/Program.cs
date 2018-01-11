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

            var records = export.Descendants("Record")
                .Select(Record.FromXElement)
                .GroupBy(r => r.Type)
                .ToDictionary(g => g.Key, g => g.AsEnumerable());

            var workouts = export.Descendants("Workout")
                .Select(Workout.FromXElement)
                .GroupBy(r => r.WorkoutType)
                .ToDictionary(g => g.Key, g => g.AsEnumerable());

            var dailySteps = PrioritizeSteps(records[HKConstants.Records.StepCount])
                .GroupBy(s => s.StartDate.Date)
                .Select(x => new
                {
                    date = x.Key,
                    steps = x.Sum(r => r.Value.SafeParse(0))
                });

            //workouts[HKConstants.Workouts.Strength]
            //    .OrderBy(w => w.StartDate)
            //    .Select(w => $"{w.StartDate} - {w.SourceName} - {w.Duration}")
            //    .ToList().ForEach(Console.WriteLine);

            workouts[HKConstants.Workouts.Cycling]
                .OrderBy(w => w.StartDate)
                .Select(w => $"{w.StartDate} - {w.SourceName} - {w.TotalDistance}")
                .ToList().ForEach(Console.WriteLine);

            //dailySteps
            //    .Select(m => $"{m.date} {m.steps}")
            //    .ToList().ForEach(Console.WriteLine);
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
}
