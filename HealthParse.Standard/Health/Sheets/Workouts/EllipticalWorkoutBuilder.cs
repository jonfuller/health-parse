using System.Collections.Generic;
using NodaTime;

namespace HealthParse.Standard.Health.Sheets.Workouts
{
    public class EllipticalWorkoutBuilder : WorkoutBuilder
    {
        public EllipticalWorkoutBuilder(IEnumerable<Workout> workouts, DateTimeZone zone, Settings.Settings settings)
            : base(workouts, HKConstants.Workouts.Elliptical, ColumnNames.Workout.Elliptical(), zone, settings)
        {
        }
    }
}