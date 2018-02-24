using System.Collections.Generic;
using NodaTime;

namespace HealthParse.Standard.Health.Sheets
{
    public class EllipticalWorkoutBuilder : WorkoutBuilder
    {
        public EllipticalWorkoutBuilder(IEnumerable<Workout> workouts, DateTimeZone zone, Settings.Settings settings)
            : base(workouts, HKConstants.Workouts.Elliptical, zone, r => new
                {
                    date = r.StartDate.InZone(zone),
                    duration = r.Duration.As(settings.DurationUnit),
                    burn = r.Energy.As(settings.EnergyUnit),
                },
                ColumnNames.Date(),
                ColumnNames.Duration(settings.DurationUnit),
                ColumnNames.EnergyBurned(settings.EnergyUnit))
        {
        }
    }
}