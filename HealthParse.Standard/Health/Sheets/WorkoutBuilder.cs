using System;
using System.Collections.Generic;
using System.Linq;
using OfficeOpenXml;

namespace HealthParse.Standard.Health.Sheets
{
    public abstract class WorkoutBuilder : ISheetBuilder<WorkoutBuilder.MonthlyWorkout>
    {
        protected Dictionary<string, IEnumerable<Workout>> _workouts;
        private Func<Workout, object> _selector;
        private string _workoutKey;

        public WorkoutBuilder(Dictionary<string, IEnumerable<Workout>> workouts, string workoutKey, Func<Workout, object> selector)
        {
            _workouts = workouts;
            _selector = selector;
            _workoutKey = workoutKey;
        }

        void ISheetBuilder.Build(ExcelWorksheet sheet)
        {
            var workouts = _workouts[_workoutKey]
                 .OrderByDescending(r => r.StartDate)
                 .Select(_selector);

            sheet.WriteData(workouts);
        }

        IEnumerable<MonthlyWorkout> ISheetBuilder<MonthlyWorkout>.BuildSummary()
        {
            return _workouts[_workoutKey]
                .GroupBy(s => new { s.StartDate.Date.Year, s.StartDate.Date.Month })
                .Select(x => new MonthlyWorkout(x.Key.Year, x.Key.Month, x.Sum(c => c.TotalDistance ?? 0), x.Sum(c => c.Duration ?? 0)));
        }

        public class MonthlyWorkout : MonthlyItem
        {
            public MonthlyWorkout(int year, int month, double distance, double duration) : base(year, month)
            {
                Distance = distance;
                Duration = duration;
            }

            public double Distance { get; }
            public double Duration { get; }
        }
    }
}
