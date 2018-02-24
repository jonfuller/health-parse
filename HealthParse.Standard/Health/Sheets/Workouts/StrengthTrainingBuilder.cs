using System.Collections.Generic;
using NodaTime;

namespace HealthParse.Standard.Health.Sheets.Workouts
{
    public class StrengthTrainingBuilder : WorkoutBuilder
    {
        public StrengthTrainingBuilder(IEnumerable<Workout> workouts, DateTimeZone zone, Settings.Settings settings)
            : base(workouts, HKConstants.Workouts.Strength, zone, r => new
            {
                date = r.StartDate.InZone(zone),
                duration = r.Duration.As(settings.DurationUnit),
            },
            ColumnNames.Date(),
            ColumnNames.Duration(settings.DurationUnit))
        {
        }
    }
}
