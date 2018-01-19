using System.Collections.Generic;

namespace HealthParse.Standard.Health.Sheets
{
    public class CyclingWorkoutBuilder : WorkoutBuilder
    {
        public CyclingWorkoutBuilder(IReadOnlyDictionary<string, IEnumerable<Workout>> workouts)
            : base(workouts, HKConstants.Workouts.Cycling, r => new
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
