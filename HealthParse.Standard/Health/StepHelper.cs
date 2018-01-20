using System.Collections.Generic;
using System.Linq;

namespace HealthParse.Standard.Health
{
    public static class StepHelper
    {
        public static IEnumerable<Record> PrioritizeSteps(IEnumerable<Record> allTheSteps)
        {
            var justSteps = allTheSteps.OrderBy(r => r.StartDate).ToList();

            for (var i = 0; i < justSteps.Count; i++)
            {
                var current = justSteps[i];
                var next = justSteps.Skip(i + 1).FirstOrDefault();
                var nextOverlaps = next != null && current.DateRange.Includes(next.StartDate, Clusivity.Inclusive);

                if (nextOverlaps)
                {
                    var loser = PickStepRecordToReject(current, next);

                    justSteps.Remove(loser);
                    i--;
                }
                else
                {
                    yield return current;
                }
            }
        }

        private static Record PickStepRecordToReject(Record a, Record b)
        {
            var records = new[] {a, b};
            var aValue = a.Value.SafeParse(0);
            var bValue = b.Value.SafeParse(0);

            var aSource = a.Raw.Attribute("sourceName").Value;
            var bSource = b.Raw.Attribute("sourceName").Value;

            if (aSource == bSource)
            {
                // same source: reject record with lower steps
                return PickSmaller(a, b);
            }

            if (aSource.Contains("Watch") || bSource.Contains("Watch"))
            {
                // we have a watch source: reject the other one
                return aSource.Contains("Watch")
                    ? b
                    : a;
            }

            // otherwise, reject the smaller one
            return PickSmaller(a, b);
        }
        private static Record PickSmaller(Record a, Record b)
        {
            var aValue = a.Value.SafeParse(0);
            var bValue = b.Value.SafeParse(0);

            return aValue > bValue ? b : a;
        }
    }
}
