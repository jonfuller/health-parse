using System.Collections.Generic;
using NodaTime;

namespace HealthParse.Standard.Health.Sheets.Workouts
{
    public class RunningWorkoutBuilder : WorkoutBuilder
    {
        public RunningWorkoutBuilder(IEnumerable<Workout> workouts, DateTimeZone zone, Settings.Settings settings)
            : base(workouts, HKConstants.Workouts.Running, ColumnNames.Workout.Running(), zone, settings)
        {
        }
    }
}
