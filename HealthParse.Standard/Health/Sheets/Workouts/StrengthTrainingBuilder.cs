using System.Collections.Generic;
using NodaTime;

namespace HealthParse.Standard.Health.Sheets.Workouts
{
    public class StrengthTrainingBuilder : WorkoutBuilder
    {
        public StrengthTrainingBuilder(IEnumerable<Workout> workouts, DateTimeZone zone, Settings.Settings settings)
            : base(workouts, HKConstants.Workouts.Strength, ColumnNames.Workout.StrengthTraining(), zone, settings)
        {
        }
    }
}
