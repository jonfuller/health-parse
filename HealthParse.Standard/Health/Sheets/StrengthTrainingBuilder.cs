using System.Collections.Generic;
using NodaTime;

namespace HealthParse.Standard.Health.Sheets
{
    public class StrengthTrainingBuilder : WorkoutBuilder
    {
        public StrengthTrainingBuilder(IReadOnlyDictionary<string, IEnumerable<Workout>> workouts, DateTimeZone zone)
            : base(workouts, HKConstants.Workouts.Strength, zone, r => new
            {
                date = r.StartDate.InZone(zone),
                duration = r.Duration,
            })
        {
        }
    }
}
