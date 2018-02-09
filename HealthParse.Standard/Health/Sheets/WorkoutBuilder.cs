using System;
using System.Collections.Generic;
using System.Linq;
using NodaTime;

namespace HealthParse.Standard.Health.Sheets
{
    public abstract class WorkoutBuilder : ISheetBuilder<WorkoutBuilder.WorkoutItem>
    {
        private readonly IEnumerable<Workout> _workouts;
        private readonly DateTimeZone _zone;
        private readonly Func<Workout, object> _selector;

        protected WorkoutBuilder(IReadOnlyDictionary<string, IEnumerable<Workout>> workouts, string workoutKey, DateTimeZone zone, Func<Workout, object> selector)
        {
            _workouts = workouts.ContainsKey(workoutKey)
                ? workouts[workoutKey]
                : Enumerable.Empty<Workout>();
            _zone = zone;
            _selector = selector;
        }

        IEnumerable<object> ISheetBuilder.BuildRawSheet()
        {
            return _workouts
                 .OrderByDescending(r => r.StartDate)
                 .Select(_selector);
        }

        IEnumerable<WorkoutItem> ISheetBuilder<WorkoutItem>.BuildSummary()
        {
            return _workouts
                .GroupBy(s => new { s.StartDate.InZone(_zone).Date.Year, s.StartDate.InZone(_zone).Date.Month })
                .Select(x => new WorkoutItem(x.Key.Year, x.Key.Month, x.Sum(c => c.TotalDistance ?? 0), x.Sum(c => c.Duration ?? 0)));
        }

        IEnumerable<WorkoutItem> ISheetBuilder<WorkoutItem>.BuildSummaryForDateRange(IRange<ZonedDateTime> dateRange)
        {
            return _workouts
                .Where(x => dateRange.Includes(x.StartDate.InZone(_zone), Clusivity.Inclusive))
                .GroupBy(x => x.StartDate.InZone(_zone).Date)
                .Select(x => new WorkoutItem(x.Key, x.Sum(c => c.TotalDistance) ?? 0, x.Sum(c => c.Duration) ?? 0))
                .OrderByDescending(x => x.Date);
        }

        public class WorkoutItem : DatedItem
        {
            public WorkoutItem(LocalDate date, double distance, double duration) : base(date)
            {
                Distance = distance;
                Duration = duration;
            }

            public WorkoutItem(int year, int month, double distance, double duration) : base(year, month)
            {
                Distance = distance;
                Duration = duration;
            }

            public double Distance { get; }
            public double Duration { get; }
        }
    }
}
