using OfficeOpenXml;
using System.Collections.Generic;
using System.Linq;

namespace HealthParse.Standard.Health
{
    public class StepBuilder : ISheetBuilder
    {
        private Dictionary<string, IEnumerable<Record>> _records;

        public StepBuilder(Dictionary<string, IEnumerable<Record>> records)
        {
            _records = records;
        }

        void ISheetBuilder.Build(ExcelWorksheet sheet)
        {
            var steps = PrioritizeSteps(_records[HKConstants.Records.StepCount])
                .GroupBy(s => s.StartDate.Date)
                .Select(x => new
                {
                    date = x.Key,
                    steps = x.Sum(r => r.Value.SafeParse(0))
                })
                .OrderByDescending(s => s.date);

            sheet.WriteData(steps);
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
    }
}
