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
                var nextOverlaps = next != null && current.DateRange.Includes(next.StartDate, Clusivity.LowerInclusive);

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
            if (a.SourceName == b.SourceName)
            {
                // same source: reject record with lower steps
                return PickSmaller(a, b);
            }

            if (a.SourceName.Contains("Watch") || b.SourceName.Contains("Watch"))
            {
                // we have a watch source: reject the other one
                return a.SourceName.Contains("Watch")
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
