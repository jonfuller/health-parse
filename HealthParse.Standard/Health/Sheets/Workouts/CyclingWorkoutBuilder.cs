using System.Collections.Generic;
using NodaTime;

namespace HealthParse.Standard.Health.Sheets.Workouts
{
    public class CyclingWorkoutBuilder : WorkoutBuilder
    {
        public CyclingWorkoutBuilder(IEnumerable<Workout> workouts, DateTimeZone zone, Settings.Settings settings)
            : base(workouts, HKConstants.Workouts.Cycling, ColumnNames.Workout.Cycling(), zone, settings)
        {
        }
    }
}
