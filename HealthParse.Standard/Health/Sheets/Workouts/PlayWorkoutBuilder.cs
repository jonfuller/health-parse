using System.Collections.Generic;
using NodaTime;

namespace HealthParse.Standard.Health.Sheets.Workouts
{
    public class PlayWorkoutBuilder : WorkoutBuilder
    {
        public PlayWorkoutBuilder(IEnumerable<Workout> workouts, DateTimeZone zone, Settings.Settings settings)
            : base(workouts, HKConstants.Workouts.Play, ColumnNames.Workout.Play(), zone, settings)
        {
        }
    }
}