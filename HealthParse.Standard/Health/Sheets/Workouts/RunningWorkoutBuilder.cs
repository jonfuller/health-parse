using System.Collections.Generic;
using NodaTime;

namespace HealthParse.Standard.Health.Sheets.Workouts
{
    public class RunningWorkoutBuilder : WorkoutBuilder
    {
        public RunningWorkoutBuilder(IEnumerable<Workout> workouts, DateTimeZone zone, Settings.Settings settings)
            : base(workouts, HKConstants.Workouts.Running, zone, r => new
                {
                    date = r.StartDate.InZone(zone),
                    duration = r.Duration.As(settings.DurationUnit),
                    distance = r.Distance.As(settings.DistanceUnit),
                },
                ColumnNames.Date(),
                ColumnNames.Duration(settings.DurationUnit),
                ColumnNames.Distance(settings.DistanceUnit))
        {
        }
    }
}
