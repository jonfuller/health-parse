using System.Collections.Generic;

namespace HealthParse.Standard.Health.Sheets
{
    public class WalkingWorkoutBuilder : WorkoutBuilder
    {
        public WalkingWorkoutBuilder(IReadOnlyDictionary<string, IEnumerable<Workout>> workouts)
            : base(workouts, HKConstants.Workouts.Walking, r => new
            {
                date = r.StartDate,
                duration = r.Duration,
                durationUnit = r.DurationUnit,
                distance = r.TotalDistance,
                unit = r.TotalDistanceUnit,
            })
        {
        }
    }
}
