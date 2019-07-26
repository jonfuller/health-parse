using System.Collections.Generic;
using System.Linq;
using NodaTime;

namespace HealthParse.Standard.Health.Sheets.Workouts
{
    public class WorkoutBuilderFactory
    {
        private readonly IEnumerable<Workout> _workouts;
        private readonly DateTimeZone _zone;
        private readonly Settings.Settings _settings;

        public WorkoutBuilderFactory(IEnumerable<Workout> workouts, DateTimeZone zone, Settings.Settings settings)
        {
            _workouts = workouts;
            _zone = zone;
            _settings = settings;
        }
        public IEnumerable<WorkoutBuilder> GetWorkoutBuilders()
        {
            return _workouts
                .GroupBy(w => w.WorkoutType)
                .Select(g => new WorkoutBuilder(g, g.Key, ColumnNames.Workout.For(g.Key), _zone, _settings));
        }
        
    }
}