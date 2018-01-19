using System.Collections.Generic;

namespace HealthParse.Standard.Health.Sheets
{
    public class RunningWorkoutBuilder : WorkoutBuilder
    {
        public RunningWorkoutBuilder(IReadOnlyDictionary<string, IEnumerable<Workout>> workouts)
            : base(workouts, HKConstants.Workouts.Running, r => new
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
