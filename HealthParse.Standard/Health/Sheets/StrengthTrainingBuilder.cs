using System.Collections.Generic;

namespace HealthParse.Standard.Health.Sheets
{
    public class StrengthTrainingBuilder : WorkoutBuilder
    {
        public StrengthTrainingBuilder(IReadOnlyDictionary<string, IEnumerable<Workout>> workouts)
            : base(workouts, HKConstants.Workouts.Strength, r => new
            {
                date = r.StartDate,
                duration = r.Duration,
                durationUnit = r.DurationUnit,
            })
        {
        }
    }
}
