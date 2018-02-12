using System.Collections.Generic;
using NodaTime;

namespace HealthParse.Standard.Health.Sheets
{
    public class WalkingWorkoutBuilder : WorkoutBuilder
    {
        public WalkingWorkoutBuilder(IReadOnlyDictionary<string, IEnumerable<Workout>> workouts, DateTimeZone zone)
            : base(workouts, HKConstants.Workouts.Walking, zone, r => new
            {
                date = r.StartDate.InZone(zone),
                duration = r.Duration,
                distance = r.Distance,
            })
        {
        }
    }
}
