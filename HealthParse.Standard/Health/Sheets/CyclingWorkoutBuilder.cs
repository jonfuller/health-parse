using System.Collections.Generic;
using NodaTime;

namespace HealthParse.Standard.Health.Sheets
{
    public class CyclingWorkoutBuilder : WorkoutBuilder
    {
        public CyclingWorkoutBuilder(IReadOnlyDictionary<string, IEnumerable<Workout>> workouts, DateTimeZone zone, Settings.Settings settings)
            : base(workouts, HKConstants.Workouts.Cycling, zone, r => new
            {
                date = r.StartDate.InZone(zone),
                duration = r.Duration.As(settings.DurationUnit),
                distance = r.Distance.As(settings.DistanceUnit),
            },
            "Date", $"Duration ({settings.DurationUnit})", $"Distance ({settings.DistanceUnit})")
        {
        }
    }
}
