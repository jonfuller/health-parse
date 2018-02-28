using System.Collections.Generic;
using NodaTime;

namespace HealthParse.Standard.Health.Sheets.Workouts
{
    public class HiitBuilder : WorkoutBuilder
    {
        public HiitBuilder(IEnumerable<Workout> workouts, DateTimeZone zone, Settings.Settings settings)
            : base(workouts, HKConstants.Workouts.Hiit, ColumnNames.Workout.HIIT(), zone, settings)
        {
        }
    }
}