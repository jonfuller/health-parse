using System.Collections.Generic;
using NodaTime;

namespace HealthParse.Standard.Health.Sheets.Workouts
{
    public class WalkingWorkoutBuilder : WorkoutBuilder
    {
        public WalkingWorkoutBuilder(IEnumerable<Workout> workouts, DateTimeZone zone, Settings.Settings settings)
            : base(workouts, HKConstants.Workouts.Walking, ColumnNames.Workout.Walking(), zone, settings)
        {
        }
    }
}
