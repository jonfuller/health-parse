using System;
using System.Collections.Generic;
using System.Linq;
using NodaTime;
using UnitsNet;
using UNDuration = UnitsNet.Duration;

namespace HealthParse.Standard.Health.Sheets
{
    public abstract class WorkoutBuilder : ISheetBuilder<WorkoutBuilder.WorkoutItem>
    {
        private readonly IEnumerable<Workout> _workouts;
        private readonly DateTimeZone _zone;
        private readonly Func<Workout, object> _selector;
        public IEnumerable<string> Headers { get; }
        public bool HasHeaders { get; }

        protected WorkoutBuilder(IReadOnlyDictionary<string, IEnumerable<Workout>> workouts, string workoutKey, DateTimeZone zone, Func<Workout, object> selector, params string[] columnNames)
        {
            _workouts = workouts.ContainsKey(workoutKey)
                ? workouts[workoutKey]
                : Enumerable.Empty<Workout>();
            _zone = zone;
            _selector = selector;
            HasHeaders = columnNames.Any();
            Headers = columnNames ?? new string[0];
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
                .Select(x => new WorkoutItem(x.Key.Year, x.Key.Month, x.SumLength(c => c.Distance), x.SumDuration(c => c.Duration)));
        }

        IEnumerable<WorkoutItem> ISheetBuilder<WorkoutItem>.BuildSummaryForDateRange(IRange<ZonedDateTime> dateRange)
        {
            return _workouts
                .Where(x => dateRange.Includes(x.StartDate.InZone(_zone), Clusivity.Inclusive))
                .GroupBy(x => x.StartDate.InZone(_zone).Date)
                .Select(x => new WorkoutItem(x.Key, x.SumLength(c => c.Distance), x.SumDuration(c => c.Duration)))
                .OrderByDescending(x => x.Date);
        }


        public class WorkoutItem : DatedItem
        {
            public WorkoutItem(LocalDate date, Length distance, UNDuration duration) : base(date)
            {
                Distance = distance;
                Duration = duration;
            }

            public WorkoutItem(int year, int month, Length distance, UNDuration duration) : base(year, month)
            {
                Distance = distance;
                Duration = duration;
            }

            public Length Distance { get; }
            public UNDuration Duration { get; }
        }
    }

    public static class Ext
    {
        public static Length SumLength<T>(this IEnumerable<T> target, Func<T, Length> selector)
        {
            return target.Aggregate(Length.Zero, (current, item) => current + selector(item));
        }
        public static UNDuration SumDuration<T>(this IEnumerable<T> target, Func<T, UNDuration> selector)
        {
            return target.Aggregate(UNDuration.Zero, (current, item) => current + selector(item));
        }
    }
}
